import { useState } from 'react'
import { assistantSuggestions } from '../../config/navigation'
import { useAuth } from "../../hooks/useAuth";

export default function AssistantPanel() {
  const { user } = useAuth()
  const [prompt, setPrompt] = useState('')
  const displayName = user?.companyName || user?.userName || '사용자'

  function handleSubmit(event) {
    event.preventDefault()
    setPrompt('')
  }

  return (
    <aside className="dashboard-panel assistant-panel">
      <div className="assistant-body" aria-label="AI assistant conversation area" />

      <div className="assistant-footer">
        <p className="assistant-greeting">
          Welcome {displayName}. How can I help you?
        </p>

        <form className="assistant-input-row" onSubmit={handleSubmit}>
          <button type="button" className="icon-button add-button" aria-label="첨부">
            +
          </button>
          <input
            type="text"
            value={prompt}
            onChange={(event) => setPrompt(event.target.value)}
            placeholder="PackageView에게 물어보기"
          />
        </form>

        <div className="assistant-suggestions">
          {assistantSuggestions.map((text) => (
            <button
              key={text}
              type="button"
              className="suggestion-chip"
              onClick={() => setPrompt(text)}
            >
              {text}
            </button>
          ))}
        </div>
      </div>
    </aside>
  )
}
