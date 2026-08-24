export const navigationGroups = [
  {
    id: 'assessment',
    label: '기본평가',
    items: [
      { id: 'usage-history', label: '사용 기록', icon: 'history' },
      { id: 'start-project', label: '프로젝트 시작', icon: 'grid' },
      { id: 'mock-assessment', label: '모의평가', icon: 'checklist' },
      { id: 'td', label: 'TD (기술문서)', icon: 'document' },
      { id: 'doc', label: 'DOC (적합성 선언서)', icon: 'badge' },
    ],
  },
  {
    id: 'library',
    label: '라이브러리',
    items: [
      { id: 'material', label: '소재물성', icon: 'beaker' },
      { id: 'process-map', label: '공정도', icon: 'flow' },
      { id: 'carbon', label: '탄소배출량', icon: 'cloud' },
      { id: 'regulation', label: '환경규제', icon: 'regulation' },
      { id: 'template', label: '디자인 템플릿', icon: 'template' },
    ],
  },
]

export const assistantSuggestions = [
  '어떻게 사용하나요?',
  '최근 업데이트된 소식',
  '문제를 해결해줘요',
]

export function findNavItem(itemId) {
  for (const group of navigationGroups) {
    const item = group.items.find((entry) => entry.id === itemId)
    if (item) {
      return { group, item }
    }
  }

  return null
}
