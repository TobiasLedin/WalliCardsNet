/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './**/*.{razor,html}',
        './**/(Layout|Pages)/*.{razor,html}',
    ],
  theme: {
      extend: {
          fontFamily: {
              roboto: ['Roboto', 'sans-serif'],
              montserrat: ['Montserrat', 'sans-serif']
          },
        },
  },
  plugins: [],
}

