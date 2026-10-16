export const navigationGroups = [
  {
    id: 'assessment',
    label: '기본평가',
    items: [
      { id: 'project-history', label: '프로젝트 현황', icon: 'history' },
        { id: 'prjdefault', label: '기본항목', icon: 'grid' },
        { id: 'prjtemplate', label: '디자인템플릿', icon: 'grid'  },
        { id: 'prjdefaultresult', label: '기본항목결과', icon: 'grid' },
        { id: 'prjeval', label: '모의평가', icon: 'checklist' },
        { id: 'prjevalresult', label: '모의평가결과', icon: 'checklist' },
        { id: 'PrjaiImage', label: 'AI이미지생성', icon: 'checklist' },
      { id: 'td', label: 'TD (기술문서)', icon: 'document' },
      { id: 'doc', label: 'DOC (적합성 선언서)', icon: 'badge' },
    ],
  },
  {
    id: 'library',
    label: '라이브러리',
    items: [
      { id: 'material9', label: '소재물성', icon: 'beaker' },
      { id: 'process-map9', label: '공정도', icon: 'flow' },
      { id: 'carbon9', label: '탄소배출량', icon: 'cloud' },
      { id: 'regulation9', label: '환경규제', icon: 'regulation' },
      { id: 'template9', label: '디자인 템플릿', icon: 'template' },
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
