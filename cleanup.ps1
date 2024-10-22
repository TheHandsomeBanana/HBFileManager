# PowerShell script to delete all 'bin' and 'obj' folders in subdirectories

# Define the folder names you want to delete
$foldersToDelete = @("bin", "obj")

foreach ($folderName in $foldersToDelete) {
    # Get all directories named 'bin' or 'obj' recursively
    $folders = Get-ChildItem -Recurse -Directory -Filter $folderName

    foreach ($folder in $folders) {
        try {
            # Remove the directory and its contents
            Remove-Item -Recurse -Force -Path $folder.FullName
            Write-Host "Deleted: $($folder.FullName)" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to delete: $($folder.FullName)" -ForegroundColor Red
        }
    }
}