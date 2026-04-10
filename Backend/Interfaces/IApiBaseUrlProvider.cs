namespace Backend.Interfaces;

/// <summary>
/// Resolves the public API base URL for absolute links to uploaded files.
/// Prefer the current HTTP request; fall back to configuration when there is no request (e.g. background work).
/// </summary>
public interface IApiBaseUrlProvider
{
    /// <summary>Origin only (scheme + host), no trailing slash.</summary>
    string GetOrigin();

    /// <summary>Absolute URL for a file under wwwroot/uploads (e.g. <c>StudentPhotos/photo.jpg</c> or <c>School/School_1_logo.png</c>).</summary>
    string UploadsFile(string relativePathUnderUploads);

    /// <summary>Base URL for a folder under uploads (no trailing slash), e.g. <c>StudentPhotos</c> or <c>Attachments</c>.</summary>
    string UploadsFolder(string folderName);
}
