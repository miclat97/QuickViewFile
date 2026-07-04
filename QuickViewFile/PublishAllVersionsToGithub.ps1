dotnet publish -p:PublishProfile=FrameworkDependent_Github_OneDrive.pubxml; 
dotnet publish -p:PublishProfile=SelfContained_32bit.pubxml; 
dotnet publish -p:PublishProfile=SelfContained_x64.pubxml; 
Remove-Item 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_FrameworkDependent\QuickViewFile.pdb'; 
Remove-Item 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x64\QuickViewFile.pdb'; 
Remove-Item 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x86\QuickViewFile.pdb'; 
Remove-Item 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_FrameworkDependent.zip'; 
Remove-Item 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x64.zip'; 
Remove-Item 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x86.zip'; 
Compress-Archive 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_FrameworkDependent\' 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_FrameworkDependent.zip'; 
Compress-Archive 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x64\' 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x64.zip'; 
Compress-Archive 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x86\' 'C:\Users\micha\OneDrive\@QuickViewFile\QuickViewFile_x86.zip'