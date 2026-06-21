import re

with open('QuickViewFile/Models/ConfigModel.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# remove duplicate
text = re.sub(r'public int TransparentBackgroundInFullScreenMode \{ get; set; \} = 0;\s*public int TransparentBackgroundInFullScreenMode \{ get; set; \} = 0;', 'public int TransparentBackgroundInFullScreenMode { get; set; } = 0;', text)

with open('QuickViewFile/Models/ConfigModel.cs', 'w', encoding='utf-8') as f:
    f.write(text)
