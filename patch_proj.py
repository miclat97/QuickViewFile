import re

with open('QuickViewFile/QuickViewFile.csproj', 'r') as f:
    text = f.read()

text = text.replace('  <ItemGroup>\n    <Content Include="QuickViewFile.ico" />\n  </ItemGroup>', '  <ItemGroup>\n    <Content Include="QuickViewFile.ico" />\n  </ItemGroup>\n  <ItemGroup>\n    <Compile Include="SettingsWindow.xaml.cs">\n      <DependentUpon>SettingsWindow.xaml</DependentUpon>\n    </Compile>\n    <Page Include="SettingsWindow.xaml">\n      <SubType>Designer</SubType>\n      <Generator>MSBuild:Compile</Generator>\n    </Page>\n  </ItemGroup>')

with open('QuickViewFile/QuickViewFile.csproj', 'w') as f:
    f.write(text)
