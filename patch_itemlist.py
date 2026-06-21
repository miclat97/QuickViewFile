with open('QuickViewFile/Models/ItemList.cs', 'r') as f:
    text = f.read()

text = text.replace(
    'public FileContentModel FileContentModel { get; set; } = new FileContentModel(); // Lazy loaded file',
    'public FileContentModel FileContentModel { get; set; } = new FileContentModel(); // Lazy loaded file\n        public System.DateTime LastModified { get; set; }\n        public string LastModifiedString { get; set; }\n        public double SizeBytes { get; set; }'
)

with open('QuickViewFile/Models/ItemList.cs', 'w') as f:
    f.write(text)
