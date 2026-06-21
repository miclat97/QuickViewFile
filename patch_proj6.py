import re

with open('QuickViewFile/QuickViewFile.csproj', 'r') as f:
    text = f.read()

# Remove the explicit Page element.
text = re.sub(r'<Page Include="SettingsWindow.xaml">\s*<SubType>Designer</SubType>\s*<Generator>MSBuild:Compile</Generator>\s*</Page>', '', text)
text = text.replace('<ItemGroup>\n    \n    \n  </ItemGroup>', '')
with open('QuickViewFile/QuickViewFile.csproj', 'w') as f:
    f.write(text)
