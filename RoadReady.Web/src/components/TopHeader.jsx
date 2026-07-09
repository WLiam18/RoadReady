import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ChevronDownIcon, MapPinIcon, LogOutIcon } from './icons';

export default function TopHeader({ location = 'India' }) {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(false);
  const [loc, setLoc] = useState(location);
  const [draft, setDraft] = useState(location);

  return (
    <header className="bg-white border-b border-brand-divider sticky top-0 z-30">
      <div className="flex items-center justify-between px-4 lg:px-6 py-3 gap-4">
        <div className="lg:hidden font-bold text-brand-ink">
          <span className="text-brand-gold">●</span> RoadReady
        </div>

        {isAuthenticated ? (
          <div className="flex items-center gap-3 ml-auto">
            <div className="hidden md:flex items-center gap-2 text-sm text-brand-ink bg-brand-surfaceAlt px-3 py-2 rounded-lg">
              <MapPinIcon className="w-4 h-4 text-brand-muted" />
              {editing ? (
                <input
                  autoFocus
                  value={draft}
                  onChange={(e) => setDraft(e.target.value)}
                  onBlur={() => {
                    if (draft.trim()) setLoc(draft.trim());
                    setEditing(false);
                  }}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') e.currentTarget.blur();
                    if (e.key === 'Escape') {
                      setDraft(loc);
                      setEditing(false);
                    }
                  }}
                  className="bg-transparent outline-none text-sm w-36"
                />
              ) : (
                <span className="cursor-pointer" onClick={() => { setDraft(loc); setEditing(true); }}>
                  {loc}
                </span>
              )}
            </div>

            <div className="relative">
              <button
                onClick={() => setOpen((v) => !v)}
                className="flex items-center gap-2 px-2 py-1 rounded-full hover:bg-brand-bg transition"
              >
                <div className="w-9 h-9 rounded-full bg-brand-ink text-white flex items-center justify-center font-bold uppercase">
                  {user?.firstName?.[0] || 'A'}
                </div>
                <ChevronDownIcon className={`w-4 h-4 transition-transform ${open ? 'rotate-180' : ''}`} />
              </button>
              {open && (
                <div
                  className="absolute right-0 mt-2 w-48 bg-white border border-brand-divider rounded-lg shadow-medium py-2 z-50"
                  onClick={() => setOpen(false)}
                >
                  <button
                    onClick={() => navigate('/profile')}
                    className="w-full text-left px-4 py-2 text-sm hover:bg-brand-bg"
                  >
                    My profile
                  </button>
                  <button
                    onClick={() => {
                      logout();
                      navigate('/');
                    }}
                    className="w-full text-left px-4 py-2 text-sm text-brand-danger hover:bg-red-50 flex items-center gap-2"
                  >
                    <LogOutIcon className="w-4 h-4" /> Log out
                  </button>
                </div>
              )}
            </div>
          </div>
        ) : (
          <div className="flex items-center gap-2 ml-auto">
            <button onClick={() => navigate('/login')} className="btn btn-ghost text-sm">
              Log in
            </button>
            <button onClick={() => navigate('/register')} className="btn btn-primary text-sm">
              Sign up
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
