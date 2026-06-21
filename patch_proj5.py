import re

with open('QuickViewFile/QuickViewFile.csproj', 'r') as f:
    text = f.read()

# Remove the explicit Compile element since default compile is enabled in SDK projects.
text = re.sub(r'<Compile Include="SettingsWindow.xaml.cs">\s*<DependentUpon>SettingsWindow.xaml</DependentUpon>\s*</Compile>', '', text)
text = re.sub(r'<EnableWindowsTargeting>true</EnableWindowsTargeting>\s*', '', text)
text = text.replace('<AllowUnsafeBlocks>False</AllowUnsafeBlocks>', '<AllowUnsafeBlocks>False</AllowUnsafeBlocks>\n    <EnableWindowsTargeting>true</EnableWindowsTargeting>')

with open('QuickViewFile/QuickViewFile.csproj', 'w') as f:
    f.write(text)
