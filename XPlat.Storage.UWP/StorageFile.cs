﻿using System.Runtime.InteropServices.WindowsRuntime;

namespace XPlat.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Windows.Storage;

    using XPlat.Storage.FileProperties;

    /// <summary>
    /// Defines an application file.
    /// </summary>
    public sealed class StorageFile : IStorageFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageFile"/> class.
        /// </summary>
        /// <param name="parentFolder">
        /// The parent folder.
        /// </param>
        /// <param name="file">
        /// The associated <see cref="StorageFile"/>.
        /// </param>
        public StorageFile(IStorageFolder parentFolder, Windows.Storage.StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            this.Originator = file;
            this.Parent = parentFolder;
        }

        /// <summary>
        /// Gets the originating Windows storage file.
        /// </summary>
        public Windows.Storage.StorageFile Originator { get; }

        /// <inheritdoc />
        public DateTime DateCreated => this.Originator.DateCreated.DateTime;

        /// <inheritdoc />
        public string Name => this.Originator.Name;

        /// <inheritdoc />
        public string DisplayName => this.Originator.DisplayName;

        /// <inheritdoc />
        public string Path => this.Originator.Path;

        /// <inheritdoc />
        public bool Exists => this.Originator != null && File.Exists(this.Path);

        /// <inheritdoc />
        public string FileType => this.Originator.FileType;

        /// <inheritdoc />
        public string ContentType => this.Originator.ContentType;

        /// <inheritdoc />
        public IStorageFolder Parent { get; private set; }

        /// <inheritdoc />
        public Task RenameAsync(string desiredName)
        {
            return this.RenameAsync(desiredName, NameCollisionOption.FailIfExists);
        }

        /// <inheritdoc />
        public async Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot rename a file that does not exist.");
            }

            if (string.IsNullOrWhiteSpace(desiredName))
            {
                throw new ArgumentNullException(nameof(desiredName));
            }

            await this.Originator.RenameAsync(desiredName, option.ToNameCollisionOption());
        }

        /// <inheritdoc />
        public async Task DeleteAsync()
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot delete a file that does not exist.");
            }

            await this.Originator.DeleteAsync();
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, object>> GetPropertiesAsync()
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(
                          this.Name,
                          "Cannot get properties for a file that does not exist.");
            }

            var storageFile = this.Originator;
            return storageFile != null ? await storageFile.Properties.RetrievePropertiesAsync(null) : null;
        }

        /// <inheritdoc />
        public bool IsOfType(StorageItemTypes type)
        {
            return type == StorageItemTypes.File;
        }

        /// <inheritdoc />
        public async Task<IBasicProperties> GetBasicPropertiesAsync()
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(
                    this.Name,
                    "Cannot get properties for a folder that does not exist.");
            }

            var storageFolder = this.Originator;
            if (storageFolder == null) return null;

            var basicProperties = await storageFolder.GetBasicPropertiesAsync();
            return basicProperties.ToBasicProperties();
        }

        /// <inheritdoc />
        public async Task<Stream> OpenReadAsync()
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot open a file that does not exist.");
            }

            var s = await this.Originator.OpenReadAsync();
            return s.AsStream();
        }

        /// <inheritdoc />
        public async Task<Stream> OpenAsync(FileAccessMode accessMode)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot open a file that does not exist.");
            }

            var s = await this.Originator.OpenAsync(accessMode.ToFileAccessMode());
            return s.AsStream();
        }

        /// <inheritdoc />
        public Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder)
        {
            return this.CopyAsync(destinationFolder, this.Name);
        }

        /// <inheritdoc />
        public Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            return this.CopyAsync(destinationFolder, desiredNewName, NameCollisionOption.FailIfExists);
        }

        /// <inheritdoc />
        public async Task<IStorageFile> CopyAsync(
            IStorageFolder destinationFolder,
            string desiredNewName,
            NameCollisionOption option)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot copy a file that does not exist.");
            }

            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            if (!destinationFolder.Exists)
            {
                throw new StorageItemNotFoundException(
                          destinationFolder.Name,
                          "Cannot copy a file to a folder that does not exist.");
            }

            if (string.IsNullOrWhiteSpace(desiredNewName))
            {
                throw new ArgumentNullException(nameof(desiredNewName));
            }

            var storageFolder =
                await Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(destinationFolder.Path));

            var copiedStorageFile =
                await this.Originator.CopyAsync(storageFolder, desiredNewName, option.ToNameCollisionOption());

            var copiedFile = new StorageFile(destinationFolder, copiedStorageFile);
            return copiedFile;
        }

        /// <inheritdoc />
        public async Task CopyAndReplaceAsync(IStorageFile fileToReplace)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot copy a file that does not exist.");
            }

            if (fileToReplace == null)
            {
                throw new ArgumentNullException(nameof(fileToReplace));
            }

            if (!fileToReplace.Exists)
            {
                throw new StorageItemNotFoundException(
                          fileToReplace.Name,
                          "Cannot copy to and replace a file that does not exist.");
            }

            var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileToReplace.Path);

            await this.Originator.CopyAndReplaceAsync(storageFile);
        }

        /// <inheritdoc />
        public Task MoveAsync(IStorageFolder destinationFolder)
        {
            return this.MoveAsync(destinationFolder, this.Name);
        }

        /// <inheritdoc />
        public Task MoveAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            return this.MoveAsync(destinationFolder, desiredNewName, NameCollisionOption.ReplaceExisting);
        }

        /// <inheritdoc />
        public async Task MoveAsync(
            IStorageFolder destinationFolder,
            string desiredNewName,
            NameCollisionOption option)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot move a file that does not exist.");
            }

            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            if (!destinationFolder.Exists)
            {
                throw new StorageItemNotFoundException(
                          destinationFolder.Name,
                          "Cannot move a file to a folder that does not exist.");
            }

            if (string.IsNullOrWhiteSpace(desiredNewName))
            {
                throw new ArgumentNullException(nameof(desiredNewName));
            }

            var storageFolder =
                await Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(destinationFolder.Path));

            await this.Originator.MoveAsync(storageFolder, desiredNewName, option.ToNameCollisionOption());

            this.Parent = destinationFolder;
        }

        /// <inheritdoc />
        public async Task MoveAndReplaceAsync(IStorageFile fileToReplace)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot move a file that does not exist.");
            }

            if (fileToReplace == null)
            {
                throw new ArgumentNullException(nameof(fileToReplace));
            }

            if (!fileToReplace.Exists)
            {
                throw new StorageItemNotFoundException(
                          fileToReplace.Name,
                          "Cannot move to and replace a file that does not exist.");
            }

            var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileToReplace.Path);

            await this.Originator.MoveAndReplaceAsync(storageFile);

            this.Parent = fileToReplace.Parent;
        }

        /// <inheritdoc />
        public async Task WriteTextAsync(string text)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot write to a file that does not exist.");
            }

            await FileIO.WriteTextAsync(this.Originator, text);
        }

        /// <inheritdoc />
        public async Task<string> ReadTextAsync()
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot read from a file that does not exist.");
            }

            var text = await FileIO.ReadTextAsync(this.Originator);
            return text;
        }

        /// <inheritdoc />
        public async Task WriteBytesAsync(byte[] bytes)
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot write to a file that does not exist.");
            }

            await FileIO.WriteBytesAsync(this.Originator, bytes);
        }

        /// <inheritdoc />
        public async Task<byte[]> ReadBytesAsync()
        {
            if (!this.Exists)
            {
                throw new StorageItemNotFoundException(this.Name, "Cannot read from a file that does not exist.");
            }

            var buffer = await FileIO.ReadBufferAsync(this.Originator);
            return buffer.ToArray();
        }

        /// <inheritdoc />
        public IStorageItemContentProperties Properties { get; }

        /// <summary>
        /// Gets an IAppFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">
        /// The path of the file to get a IAppFile to represent. If your path uses slashes, make sure you use backslashes (\). Forward slashes (/) are not accepted by this method.
        /// </param>
        /// <returns>
        /// When this method completes, it returns the file as an IAppFile.
        /// </returns>
        public static async Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            Windows.Storage.StorageFile pathFile;
            Windows.Storage.StorageFolder pathFileParentFolder;

            StorageFolder resultFileParentFolder;

            try
            {
                pathFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
            }
            catch (Exception)
            {
                pathFile = null;
            }

            if (pathFile == null)
            {
                return null;
            }

            try
            {
                pathFileParentFolder = await pathFile.GetParentAsync();
            }
            catch (Exception)
            {
                pathFileParentFolder = null;
            }

            if (pathFileParentFolder != null)
            {
                resultFileParentFolder = await StorageFolder.GetFolderFromPathAsync(pathFileParentFolder.Path);
            }
            else
            {
                resultFileParentFolder = null;
            }

            var resultFile = new StorageFile(resultFileParentFolder, pathFile);

            return resultFile;
        }

        /// <summary>
        /// Gets an IAppFile object to represent the file at the specified uri of the application.
        /// </summary>
        /// <param name="uri">
        /// The uri of the file to get a IAppFile to represent. 
        /// </param>
        /// <returns>
        /// When this method completes, it returns the file as an IAppFile.
        /// </returns>
        public static async Task<IStorageFile> GetFileFromApplicationUriAsync(Uri uri)
        {
            Windows.Storage.StorageFile pathFile;
            Windows.Storage.StorageFolder pathFileParentFolder;

            StorageFolder resultFileParentFolder;

            try
            {
                pathFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            catch (Exception)
            {
                pathFile = null;
            }

            if (pathFile == null)
            {
                return null;
            }

            try
            {
                pathFileParentFolder = await pathFile.GetParentAsync();
            }
            catch (Exception)
            {
                pathFileParentFolder = null;
            }

            if (pathFileParentFolder != null)
            {
                resultFileParentFolder = await StorageFolder.GetFolderFromPathAsync(pathFileParentFolder.Path);
            }
            else
            {
                resultFileParentFolder = null;
            }

            var resultFile = new StorageFile(resultFileParentFolder, pathFile);

            return resultFile;
        }
    }
}