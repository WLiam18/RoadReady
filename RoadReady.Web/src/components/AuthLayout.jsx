import { Link } from 'react-router-dom';
import { Logo } from './Logo';
import { ArrowRightIcon } from './icons';

export default function AuthLayout({ children, title, subtitle, heroImage }) {
  return (
    <div className="min-h-screen flex flex-col lg:flex-row">
      {/* Hero panel */}
      <div
        className="hidden lg:flex lg:w-1/2 relative bg-cover bg-center"
        style={{
          backgroundImage: heroImage
            ? `url(${heroImage})`
            : "linear-gradient(135deg, #0E0E10 0%, #1F2937 50%, #374151 100%)",
        }}
      >
        <div className="absolute inset-0 bg-gradient-to-b from-black/30 via-black/40 to-black/60" />
        <div className="relative z-10 p-10 flex flex-col justify-between text-white w-full">
          <Logo dark size={36} />

          <div className="max-w-md">
            <h1 className="text-4xl xl:text-5xl font-bold leading-tight tracking-tight">
              {title}
            </h1>
            {subtitle && (
              <p className="mt-4 text-white/80 text-base leading-relaxed">
                {subtitle}
              </p>
            )}
            <div className="mt-8 flex items-center gap-2 text-sm text-white/70">
              <ArrowRightIcon className="w-4 h-4" />
              <span>Premium fleet. Instant booking. Secure payment.</span>
            </div>
          </div>

          <p className="text-xs text-white/50">
            &copy; {new Date().getFullYear()} RoadReady. Drive with confidence.
          </p>
        </div>
      </div>

      {/* Form panel */}
      <div className="flex-1 lg:w-1/2 flex flex-col px-6 lg:px-16 xl:px-24 py-10 lg:py-16 bg-white">
        <div className="lg:hidden mb-6">
          <Logo size={32} />
        </div>

        <div className="max-w-md w-full mx-auto flex-1 flex flex-col justify-center">
          {children}
        </div>

        <p className="text-xs text-brand-muted text-center mt-10">
          <Link to="/" className="hover:text-brand-ink">&larr; Back to website</Link>
        </p>
      </div>
    </div>
  );
}
