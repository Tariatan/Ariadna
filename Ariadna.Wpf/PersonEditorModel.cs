using System.Windows.Media.Imaging;
using Ariadna.Storage;

namespace Ariadna.Wpf;
internal sealed class PersonEditorModel(PersonPhoto person) : ObservableObject
{
    private string name = person.Name;
    private byte[]? photo = person.Photo;
    public string Name { get => name; set => Set(ref name, value); }
    public BitmapSource? Portrait => photo == null ? null : Images.Decode(photo, 100);
    internal byte[]? Photo => photo;

    internal void SetPhoto(byte[]? value)
    {
        photo = value;
        Notify(nameof(Portrait));
    }

    internal PersonPhoto ToPerson() => new(Name, photo);
}
