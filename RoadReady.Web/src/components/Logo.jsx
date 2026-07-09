// A typographic wordmark: a solid square with the brand initial sits next to the
// wordmark. Clean and easy to recolour — when `dark` is true the square is white
// (for use on dark backgrounds), otherwise it's the brand-ink color.
export function Logo({ className = '', size = 32, dark = false }) {
  const squareBg = dark ? '#FFFFFF' : '#0E0E10';
  const squareFg = dark ? '#0E0E10' : '#FFFFFF';
  const wordColor = dark ? '#FFFFFF' : '#0E0E10';
  // Use a square + the wordmark. The square always carries the initial "R".
  return (
    <div className={`inline-flex items-center gap-3 ${className}`}>
      <div
        aria-hidden
        className="flex items-center justify-center font-extrabold select-none rounded-xl"
        style={{
          width: size,
          height: size,
          background: squareBg,
          color: squareFg,
          fontSize: Math.round(size * 0.5),
          letterSpacing: '-0.02em',
          lineHeight: 1,
        }}
      >
        R
      </div>
      <span
        className="font-extrabold tracking-tight leading-none"
        style={{
          color: wordColor,
          fontSize: Math.round(size * 0.46),
          letterSpacing: '-0.02em',
        }}
      >
        RoadReady
      </span>
    </div>
  );
}
