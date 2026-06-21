import re

with open('QuickViewFile/QuickViewFile.csproj', 'r') as f:
    text = f.read()

text = text.replace('<TargetFramework>net10.0-windows10.0.22000.0</TargetFramework>', '<TargetFramework>net10.0-windows</TargetFramework>')
text = text.replace('<AllowUnsafeBlocks>False</AllowUnsafeBlocks>', '<AllowUnsafeBlocks>False</AllowUnsafeBlocks>\n    <EnableWindowsTargeting>true</EnableWindowsTargeting>')

with open('QuickViewFile/QuickViewFile.csproj', 'w') as f:
    f.write(text)
