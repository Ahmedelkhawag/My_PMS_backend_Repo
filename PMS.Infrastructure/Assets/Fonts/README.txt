For correct Arabic text rendering in the Registration Card PDF, place an Arabic-supporting font here.

Recommended: Cairo or Tajawal from Google Fonts (https://fonts.google.com/).
- Download Cairo (or Tajawal), then add "Cairo-Regular.ttf" to this folder.
- In PMS.Infrastructure.csproj the font is set to copy to output so the PDF service can load it.

If no font is present, the PDF will still generate using the default font; Arabic may not display with proper shaping.
