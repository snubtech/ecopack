import { useEffect, useState } from 'react';
import { getProjects, createProject } from '../api/projects';

// 1. 8개 전체 수출국 목록 및 DB 필드 매핑 정의
const COUNTRIES = [
    { key: 'usa', label: '미국 (USA)', field: 'prdExpCntryNm1' },
    { key: 'eu', label: '유럽 (EU)', field: 'prdExpCntryNm2' },
    { key: 'china', label: '중국 (China)', field: 'prdExpCntryNm3' },
    { key: 'vietnam', label: '베트남 (Vietnam)', field: 'prdExpCntryNm4' },
    { key: 'indonesia', label: '인도네시아 (Indonesia)', field: 'prdExpCntryNm5' },
    { key: 'japan', label: '일본 (Japan)', field: 'prdExpCntryNm6' },
    { key: 'australia', label: '호주 (Australia)', field: 'prdExpCntryNm7' },
    { key: 'korea', label: '대한민국 (Korea)', field: 'prdExpCntryNm8' }
];

const PACKAGING_LEVELS = [
    { key: 'sales', label: '판매(1차)' },
    { key: 'group', label: '그룹(2차)' },
    { key: 'transport', label: '운송(3차)' }
];

export default function Projects() {
    const [projectList, setProjectList] = useState([]);

    // 모달 오픈/클로즈 상태 관리
    const [isModalOpen, setIsModalOpen] = useState(false);

    // 신규 프로젝트 폼 입력 상태 관리
    const [formData, setFormData] = useState({
        prjNm: '',
        exportCountry: '미국 (USA)', // 기본값 미국
        packagingLevels: { sales: true, group: false, transport: false },
        projectContent: '',
        repNm: ''
    });

    // 데이터 로드
    useEffect(() => {
        let isMounted = true;

        async function loadProjects() {
            try {
                const data = await getProjects();
                if (isMounted) {
                    setProjectList(data);
                }
            } catch (err) {
                console.error('프로젝트 목록을 불러오는 중 오류가 발생했습니다.', err);
            }
        }

        loadProjects();

        return () => {
            isMounted = false;
        };
    }, []);

    // 입력값 변경 핸들러
    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    // 포장 차수 체크박스 핸들러 (복수 선택)
    const handleCheckboxChange = (category, key) => {
        setFormData(prev => ({
            ...prev,
            [category]: {
                ...prev[category],
                [key]: !prev[category][key]
            }
        }));
    };

    // 저장 버튼 클릭 핸들러 (선택한 포장 차수 개수만큼 쪼개서 각각 전송)
    const handleSave = async () => {
        if (!formData.prjNm.trim()) {
            alert('프로젝트 이름을 입력해주세요.');
            return;
        }

        // 1. 사용자가 체크한 포장 차수를 배열로 수집 ('1': 판매, '2': 그룹, '3': 운송)
        const selectedLevels = [];
        if (formData.packagingLevels.sales) selectedLevels.push('1');
        if (formData.packagingLevels.group) selectedLevels.push('2');
        if (formData.packagingLevels.transport) selectedLevels.push('3');

        // 포장 차수를 최소 1개 이상 선택했는지 검증
        if (selectedLevels.length === 0) {
            alert('포장 차수를 최소 1개 이상 선택해주세요.');
            return;
        }

        // 2. 세션에서 로그인한 사용자 아이디 가져오기 (없으면 기본값 'admin')
        // const userId = sessionStorage.getItem('prjuserid') || 'admin';
        // 💡 세션에 객체로 들어가 있는 아이디를 문자열("user")만 깔끔하게 추출
        const rawUser = sessionStorage.getItem('prjuserid');
        const userId = rawUser && rawUser.startsWith('{') ? JSON.parse(rawUser).repCustId : (rawUser || 'user');


        // 3. 8개 국가 필드를 동적으로 'Y' 또는 'N' 매핑
        const countryDto = COUNTRIES.reduce((acc, country) => {
            acc[country.field] = formData.exportCountry === country.label ? 'Y' : 'N';
            return acc;
        }, {});

        try {
            let sharedPrjId = null; // 공통 프로젝트 번호 변수
            // 4. 선택한 차수 개수만큼 루프를 돌며 각각 독립된 데이터로 분할 전송
            for (const level of selectedLevels) {
                const newDto = {
                    prjNm: formData.prjNm,
                    repNm: formData.repNm || '담당자미정',
                    Prjmemo: formData.projectContent, // 프로젝트 내용
                    PackLevel: level,
                    prjuserid: userId,
                    // countryDto 속성들을 하나씩 명시적으로 풀어씀 
                    prdExpCntryNm1: countryDto.prdExpCntryNm1,
                    prdExpCntryNm2: countryDto.prdExpCntryNm2,
                    prdExpCntryNm3: countryDto.prdExpCntryNm3,
                    prdExpCntryNm4: countryDto.prdExpCntryNm4,
                    prdExpCntryNm5: countryDto.prdExpCntryNm5,
                    prdExpCntryNm6: countryDto.prdExpCntryNm6,
                    prdExpCntryNm7: countryDto.prdExpCntryNm7,
                    prdExpCntryNm8: countryDto.prdExpCntryNm8,

                    // 현재 반복 중인 차수만 'Y', 나머지는 'N'으로 세팅
                    prdPkgSeq1: level === '1' ? 'Y' : 'N',
                    prdPkgSeq2: level === '2' ? 'Y' : 'N',
                    prdPkgSeq3: level === '3' ? 'Y' : 'N',
                };

                // createProject가 서버 응답으로 { success: true, prjId: "..." } 를 반환한다고 가정
                const response = await createProject(newDto);

                // 첫 번째 생성 때 서버가 만들어준 PrjId를 잡아서 이후 차수들에 공통으로 적용
                //!sharedPrjId가 null이고 response존재하고. response.prjId가 있다면....
                if (!sharedPrjId && response && response.prjId) {
                    sharedPrjId = response.prjId;
                }
            }

            alert('선택한 포장 차수별로 신규 프로젝트가 성공적으로 생성되었습니다.');
            setIsModalOpen(false); // 모달 닫기

            // 5. 최근 프로젝트 이력 목록 새로고침
            const data = await getProjects();
            setProjectList(data);

            // 6. 폼 초기화 (sales는 기본값 true로 복원)
            setFormData({
                prjNm: '',
                exportCountry: '미국 (USA)',
                packagingLevels: { sales: true, group: false, transport: false },
                projectContent: '',
                repNm: ''
            });

        } catch (err) {
            alert('프로젝트 등록 중 오류가 발생했습니다.');
            console.error(err);
        }
    };

    // 💡 7. 이력 테이블에서 [수정] 버튼을 눌렀을 때 실행되는 핸들러 (세션 반영)
    const handleEditClick = (item) => {
       
        // 개별 키로 각각 저장해두는 게 편하다면 아래처럼 각각 담아도 좋습니다.
        sessionStorage.setItem('currentPrjNm', item.prjNm);
        sessionStorage.setItem('currentPrjId', item.prjId);
        sessionStorage.setItem('currentPackLevel', item.packLevel || '');
        // (선택) 디버깅용 로그 확인
        console.log("세션 저장 완료:", {
            prjNm: item.prjNm,
            prjId: item.prjId,
            packLevel: item.packLevel
        });
        // 2. 필요시 다음 단계 페이지로 이동 (예: navigate('/next-step') 등 필요한 라우터 코드가 있다면 여기에 작성)
    };

    return (
        <div style={{ padding: '40px', fontFamily: 'sans-serif', maxWidth: '1200px', margin: '0 auto', boxSizing: 'border-box' }}>

            {/* 1. 대시보드 홈 타이틀 및 설명 */}
            <div style={{ marginBottom: '20px' }}>
                <p style={{ color: '#555', fontSize: '14px', margin: 0 }}>진행 중인 프로젝트 이력과 공지사항을 확인하세요.</p>
            </div>

            {/* 2. 공지사항(NOTICE) 배너 */}
            <div style={{ backgroundColor: '#f8f9fa', border: '1px solid #e2e8f0', padding: '42px 20px', borderRadius: '6px', marginBottom: '30px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <span style={{ backgroundColor: '#198754', color: '#fff', padding: '4px 10px', borderRadius: '4px', fontSize: '12px', fontWeight: 'bold' }}>NOTICE</span>
                    <span style={{ fontSize: '14px', color: '#333' }}>[공지] 에코패키징 디자인추천 시스템개발 및 고도화작업 진행중 (2026~2027년)</span>
                </div>
                <span style={{ color: '#888', fontSize: '13px' }}>2026-04-12</span>
            </div>

            {/* 3. 중앙 정렬된 신규 프로젝트 진행 버튼 영역 */}
            <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '40px' }}>
                <button
                    onClick={() => setIsModalOpen(true)}
                    style={{ backgroundColor: '#198754', color: '#fff', border: 'none', padding: '14px 40px', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', fontSize: '16px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}
                >
                    + 신규 프로젝트 작성
                </button>
            </div>

            {/* 4. 최근 프로젝트 이력 테이블 영역 */}
            <div>
                <h3 style={{ marginBottom: '15px', fontSize: '18px', fontWeight: 'bold', color: '#222' }}>최근 프로젝트 이력</h3>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', backgroundColor: '#fff', border: '1px solid #e2e8f0' }}>
                    {/* 헤더 부분 */}
                    <thead>
                        <tr style={{ backgroundColor: '#f8f9fa', borderBottom: '1px solid #e2e8f0', color: '#444', fontSize: '14px', height: '32px' }}>
                            <th style={{ padding: '4px 8px' }}>날짜</th>
                            <th style={{ padding: '4px 8px' }}>프로젝트명</th>
                            <th style={{ padding: '4px 8px' }}>프로젝트 번호</th>
                            <th style={{ padding: '4px 8px' }}>포장 차수</th>
                            <th style={{ padding: '4px 8px' }}>담당자</th>
                            <th style={{ padding: '4px 8px' }}>진행상태</th>
                            <th style={{ padding: '4px 8px', textAlign: 'center' }}>관리</th>
                        </tr>
                    </thead>

                    {/* 데이터 바디 부분 */}
                    <tbody>
                        {projectList.length === 0 ? (
                            <tr>
                                <td colSpan="7" style={{ textAlign: 'center', padding: '20px', color: '#888' }}>
                                    등록된 프로젝트 이력이 없습니다.
                                </td>
                            </tr>
                        ) : (
                            projectList.map((item) => (
                                <tr key={item.prjId} style={{ borderBottom: '1px solid #f1f5f9', fontSize: '13px', height: '28px' }}>
                                    <td style={{ padding: '7px 8px', color: '#555' }}>{item.prjFcrtDt || '2026-04-10'}</td>
                                    <td style={{ padding: '7px 8px', fontWeight: 'bold', color: '#222' }}>{item.prjNm}</td>
                                    <td style={{ padding: '7px 8px', color: '#555' }}>{item.prjId}</td>
                                    <td style={{ padding: '7px 8px', color: '#555' }}>
                                        {item.packLevel ? `${item.packLevel}차` : '-'}
                                    </td>
                                    <td style={{ padding: '7px 8px', color: '#555' }}>{item.repNm}</td>

                                    {/* 진행상태 뱃지 */}
                                    <td style={{ padding: '7px 8px' }}>
                                        <span style={{ backgroundColor: '#e2e8f0', color: '#333', padding: '1px 6px', borderRadius: '4px', fontSize: '11px', fontWeight: '500', display: 'inline-block' }}>
                                            프로젝트생성
                                        </span>
                                    </td>

                                    {/* 수정 버튼 */}
                                    <td style={{ padding: '7px 8px', textAlign: 'center' }}>
                                        <button
                                            onClick={() => handleEditClick(item)}
                                            style={{ padding: '1px 6px', border: '1px solid #cbd5e1', backgroundColor: '#fff', borderRadius: '4px', cursor: 'pointer', fontSize: '11px', color: '#333' }}
                                        >
                                            수정
                                        </button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* ================= 신규 프로젝트 생성 모달 (Modal) ================= */}
            {isModalOpen && (
                <div style={{
                    position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh',
                    backgroundColor: 'rgba(0, 0, 0, 0.5)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 1000
                }}>
                    <div style={{
                        backgroundColor: '#fff', width: '840px', padding: '30px', borderRadius: '8px',
                        boxShadow: '0 4px 12px rgba(0,0,0,0.15)', boxSizing: 'border-box', maxHeight: '90vh', overflowY: 'auto'
                    }}>
                        {/* 모달 타이틀 */}
                        <div style={{ backgroundColor: '#386641', color: '#fff', padding: '12px 18px', fontSize: '18px', fontWeight: 'bold', borderRadius: '4px', marginBottom: '20px' }}>
                            신규 프로젝트 생성
                        </div>

                        {/* 1. 프로젝트 이름 */}
                        <div style={{ marginBottom: '20px' }}>
                            <label style={{ display: 'block', fontSize: '14px', fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>1. 프로젝트 이름</label>
                            <input
                                type="text"
                                name="prjNm"
                                value={formData.prjNm}
                                onChange={handleChange}
                                placeholder="프로젝트 이름을 입력하세요"
                                style={{ width: '100%', padding: '12px', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box', fontSize: '14px' }}
                            />
                        </div>

                        {/* 2. 제품을 수출할 국가 선택 */}
                        <div style={{ marginBottom: '20px' }}>
                            <label style={{ display: 'block', fontSize: '14px', fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>2. 제품을 수출할 국가를 선택해 주세요 (1개)</label>
                            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '8px' }}>
                                {COUNTRIES.map((country) => {
                                    const isSelected = formData.exportCountry === country.label;
                                    return (
                                        <label
                                            key={country.key}
                                            style={{
                                                padding: '10px 4px',
                                                fontSize: '12px',
                                                whiteSpace: 'nowrap',
                                                border: isSelected ? '2px solid #386641' : '1px solid #ccc',
                                                backgroundColor: isSelected ? '#f0fdf4' : '#fff',
                                                color: isSelected ? '#386641' : '#333',
                                                borderRadius: '6px',
                                                cursor: 'pointer',
                                                fontWeight: '500',
                                                display: 'flex',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                gap: '4px',
                                                overflow: 'hidden',
                                                textOverflow: 'ellipsis'
                                            }}
                                        >
                                            <input
                                                type="radio"
                                                name="exportCountry"
                                                checked={isSelected}
                                                onChange={() => setFormData(prev => ({ ...prev, exportCountry: country.label }))}
                                                style={{ width: '14px', height: '14px', cursor: 'pointer', margin: 0, flexShrink: 0 }}
                                            />
                                            <span style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{country.label}</span>
                                        </label>
                                    );
                                })}
                            </div>
                        </div>

                        {/* 3. 진단할 포장 차수 선택 */}
                        <div style={{ marginBottom: '20px' }}>
                            <label style={{ display: 'block', fontSize: '14px', fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>3. 진단할 포장 차수를 선택해 주세요 (복수 선택 가능)</label>
                            <div style={{ display: 'flex', gap: '12px' }}>
                                {PACKAGING_LEVELS.map((level) => {
                                    const isSelected = formData.packagingLevels[level.key];
                                    return (
                                        <label
                                            key={level.key}
                                            style={{
                                                flex: 1,
                                                padding: '12px 10px',
                                                fontSize: '14px',
                                                border: isSelected ? '2px solid #386641' : '1px solid #ccc',
                                                backgroundColor: isSelected ? '#f0fdf4' : '#fff',
                                                color: isSelected ? '#386641' : '#333',
                                                borderRadius: '6px',
                                                cursor: 'pointer',
                                                fontWeight: '500',
                                                display: 'flex',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                gap: '6px'
                                            }}
                                        >
                                            <input
                                                type="checkbox"
                                                checked={isSelected}
                                                onChange={() => handleCheckboxChange('packagingLevels', level.key)}
                                                style={{ width: '18px', height: '18px', cursor: 'pointer' }}
                                            />
                                            {level.label}
                                        </label>
                                    );
                                })}
                            </div>
                        </div>

                        {/* 4. 프로젝트 내용 */}
                        <div style={{ marginBottom: '20px' }}>
                            <label style={{ display: 'block', fontSize: '14px', fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>4. 프로젝트 내용</label>
                            <textarea
                                name="projectContent"
                                value={formData.projectContent}
                                onChange={handleChange}
                                rows="3"
                                placeholder="프로젝트 내용을 입력하세요"
                                style={{ width: '100%', padding: '12px', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box', resize: 'none', fontSize: '14px' }}
                            />
                        </div>

                        {/* 5. 담당자 */}
                        <div style={{ marginBottom: '30px' }}>
                            <label style={{ display: 'block', fontSize: '14px', fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>5. 담당자</label>
                            <input
                                type="text"
                                name="repNm"
                                value={formData.repNm}
                                onChange={handleChange}
                                placeholder="담당자 이름을 입력하세요"
                                style={{ width: '100%', padding: '12px', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box', fontSize: '14px' }}
                            />
                        </div>

                        {/* 하단 버튼 영역 */}
                        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
                            <button
                                onClick={handleSave}
                                style={{ backgroundColor: '#2b6cb0', color: '#fff', border: 'none', padding: '12px 25px', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold', fontSize: '15px' }}
                            >
                                저장
                            </button>
                            <button
                                onClick={() => setIsModalOpen(false)}
                                style={{ backgroundColor: '#4299e1', color: '#fff', border: 'none', padding: '12px 25px', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold', fontSize: '15px' }}
                            >
                                닫기
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}