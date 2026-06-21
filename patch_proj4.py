import re

with open('QuickViewFile/QuickViewFile.csproj', 'r') as f:
    text = f.read()

text = text.replace('<AllowUnsafeBlocks>False</AllowUnsafeBlocks>', '<AllowUnsafeBlocks>False</AllowUnsafeBlocks>\n    <EnableWindowsTargeting>true</EnableWindowsTargeting>')

with open('QuickViewFile/QuickViewFile.csproj', 'w') as f:
    f.write(text)
