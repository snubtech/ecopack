/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - shrinkImagesForDocx 유틸 (DOCX 추출용 이미지 축소)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - TD 화면의 DOCX 추출은 실제 .docx가 아니라, MS Office XML 네임스페이스를 붙인
 *      HTML을 .doc 확장자로 저장하는 방식이다 (Word의 "HTML 열기" 기능을 빌려 쓴다).
 *    - Word의 이 HTML 가져오기 엔진은 <img>의 CSS width/max-width(%, cm 모두)를
 *      거의 지키지 않고, 이미지 파일 자체의 픽셀 크기(+ 해상도 메타데이터)를 기준으로
 *      그려 버린다. 그래서 원본이 2430×1349px 같은 큰 이미지(공정도 memoImg,
 *      제조 도면 업로드본)는 CSS로 아무리 폭을 눌러도 문서 가로폭을 벗어난 크기로 나온다.
 *
 * 2. 해결 방식
 *    - CSS로 "눌러 보이게" 하는 대신, <canvas>에 그려서 이미지 파일 자체를 문서 폭에
 *      맞는 픽셀 크기로 다시 인코딩한다. 그러면 Word가 원본 크기 그대로 그려도
 *      이미 문서 폭에 맞는 크기가 된다 — CSS 해석에 기대지 않는 확실한 방법이다.
 *    - 원본이 목표 폭보다 이미 작으면 확대하지 않는다(화질 저하 방지, 그대로 둔다).
 *
 * 3. 쓰임새
 *    - handleExportDocx에서 Blob을 만들기 직전, cloneNode한 문서 안의 모든 <img>에
 *      대해 await shrinkImagesForDocx(clone, targetWidthCm)를 호출한다.
 * ==============================================================================
 */

/**
 * cm를 픽셀로 환산할 때 쓰는 기준 해상도.
 * ⚠️ 150dpi로 계산했다가 실제로는 여전히 화면을 벗어나는 크기로 나오는 걸 확인했다 — 원인은
 * canvas.toDataURL('image/png')로 새로 만든 PNG에는 해상도(dpi) 메타데이터가 안 들어있고,
 * Word는 그런 경우 <img width="..." height="...">의 픽셀 값을 CSS 표준인 96dpi 기준으로
 * 해석한다는 것. 150dpi로 픽셀 수를 계산해 attribute에 넣으면, Word가 그걸 96dpi로 도로
 * 해석해서 목표보다 150/96 ≈ 1.56배 크게 그려진다. 그래서 여기도 96으로 맞춰야
 * "픽셀 수 ÷ 96 × 2.54cm/inch"가 실제 목표 cm와 일치한다.
 */
const DOCX_IMAGE_DPI = 96;
const CM_PER_INCH = 2.54;

/**
 * 문서 안의 모든 <img>를 목표 가로폭(cm)에 맞춰 픽셀 단위로 다시 그려 넣는다.
 * data: URI가 아닌 이미지(외부 URL 등)는 손댈 수 없으니 건드리지 않고 넘어간다.
 *
 * @param {HTMLElement} root          이미지를 찾을 루트 요소 (예: cloneNode한 문서 사본)
 * @param {number}      targetWidthCm 목표 가로폭 (cm)
 * @returns {Promise<void>} 모든 이미지 처리가 끝나면 resolve
 */
export async function shrinkImagesForDocx(root, targetWidthCm) {
    const targetWidthPx = Math.round((targetWidthCm / CM_PER_INCH) * DOCX_IMAGE_DPI);
    const images = Array.from(root.querySelectorAll('img'));

    await Promise.all(images.map((img) => shrinkOneImage(img, targetWidthPx)));
}

function shrinkOneImage(img, targetWidthPx) {
    const src = img.getAttribute('src') || '';
    if (!src.startsWith('data:image/')) return Promise.resolve();

    return new Promise((resolve) => {
        const probe = new Image();
        probe.onload = () => {
            const naturalW = probe.naturalWidth || targetWidthPx;
            const naturalH = probe.naturalHeight || targetWidthPx;
            // 원본이 이미 목표 폭보다 작으면 확대하지 않고 원본 크기를 그대로 쓴다.
            const drawW = Math.min(naturalW, targetWidthPx);
            const drawH = Math.round(naturalH * (drawW / naturalW));

            const canvas = document.createElement('canvas');
            canvas.width = drawW;
            canvas.height = drawH;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(probe, 0, 0, drawW, drawH);

            try {
                const resized = canvas.toDataURL('image/png');
                img.setAttribute('src', resized);
            } catch {
                // toDataURL이 실패해도(이례적) 원본 src는 그대로 남아 있으니 export는 계속 진행한다.
            }
            // Word 버전에 따라 width/height 속성(픽셀)을 신뢰하기도, style(cm)을 신뢰하기도 해서
            // 두 가지를 같은 물리 크기가 되도록 함께 넣어 둔다 — 어느 쪽을 읽어도 결과가 같다.
            const widthCm = (drawW / DOCX_IMAGE_DPI) * CM_PER_INCH;
            img.setAttribute('width', String(drawW));
            img.setAttribute('height', String(drawH));
            img.style.width = `${widthCm.toFixed(2)}cm`;
            img.style.height = 'auto';
            img.style.maxWidth = '100%';
            resolve();
        };
        probe.onerror = () => resolve(); // 이미지 하나가 깨져도 나머지 추출은 계속 진행한다.
        probe.src = src;
    });
}
