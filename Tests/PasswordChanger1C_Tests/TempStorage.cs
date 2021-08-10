// from https://stackoverflow.com/questions/4105390/c-sharp-using-idisposable-to-clean-up-temp-files

using System;
using System.IO;

/// <summary>
/// Represents a temporary storage on file system.
/// </summary>
public sealed partial class TempStorage
     : IDisposable
{
    #region Constructor

    private TempStorage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TempStorage"/> class.
    /// </summary>
    /// <param name="path">The path to use as temp storage.</param>
    public TempStorage(string path)
    {
        this.Path = path;
        this.Clear();
        this.Create();
    }

    #endregion

    #region Properties

    private string Path
    {
        get;
        set;
    }

    #endregion

    #region Methods

    private void Create()
    {
        try
        {
            if (!Directory.Exists(this.Path))
            {
                Directory.CreateDirectory(this.Path);
            }
        }
        catch (IOException)
        {
        }
    }

    public void Clear()
    {
        try
        {
            if (Directory.Exists(this.Path))
            {
                Directory.Delete(this.Path, true);
            }
        }
        catch (IOException)
        {
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// An indicator whether this object is beeing actively disposed or not.
    /// </summary>
    private bool disposed;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Throws an exception if something is tried to be done with an already disposed object.
    /// </summary>
    /// <remarks>
    /// All public methods of the class must first call this.
    /// </remarks>
    public void ThrowIfDisposed()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }

    /// <summary>
    /// Releases managed resources upon dispose.
    /// </summary>
    /// <remarks>
    /// All managed resources must be released in this
    /// method, so after disposing this object no other
    /// object is beeing referenced by it anymore.
    /// </remarks>
    private void ReleaseManagedResources()
    {
        this.Clear();
    }

    /// <summary>
    /// Releases unmanaged resources upon dispose.
    /// </summary>
    /// <remarks>
    /// All unmanaged resources must be released in this
    /// method, so after disposing this object no other
    /// object is beeing referenced by it anymore.
    /// </remarks>
    private void ReleaseUnmanagedResources()
    {
    }

    private void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            /* Release unmanaged ressources */
            this.ReleaseUnmanagedResources();

            if (disposing)
            {
                /* Release managed ressources */
                this.ReleaseManagedResources();
            }

            /* Set indicator that this object is disposed */
            this.disposed = true;
        }
    }

    #endregion
}