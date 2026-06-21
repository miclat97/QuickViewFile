import re

with open('QuickViewFile/QuickViewFile.csproj', 'r') as f:
    text = f.read()

# Make sure we don't include SettingsWindow explicitly to avoid Duplicate Compile Error
# We will just enable windows targeting.
