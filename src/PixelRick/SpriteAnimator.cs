using System.Windows.Media.Imaging;
using System.Windows;
namespace PixelRick;
public sealed class SpriteAnimator
{
    private readonly CharacterDefinition character;
    private readonly Dictionary<string, BitmapSource[]> cache = new();
    private readonly AnimationClock timing;
    public string Current => timing.Current;
    public double Elapsed => timing.Elapsed;
    public bool Finished => timing.Finished;
    public AnimationDefinition Definition => timing.Definition;
    public SpriteAnimator(CharacterDefinition character, AnimationClock? clock = null)
    {
        this.character = character; timing = clock ?? new(character);
        foreach (var (key, a) in character.Animations)
        {
            var bmp=new BitmapImage();bmp.BeginInit();bmp.UriSource=new Uri(System.IO.Path.Combine(character.Root,a.File));bmp.CacheOption=BitmapCacheOption.OnLoad;bmp.EndInit();bmp.Freeze();
            if(bmp.PixelWidth!=a.FrameWidth*a.Frames || bmp.PixelHeight!=a.FrameHeight)throw new System.IO.InvalidDataException(key);
            cache[key]=Enumerable.Range(0,a.Frames).Select(i=> { var frame=new CroppedBitmap(bmp,new Int32Rect(i*a.FrameWidth,0,a.FrameWidth,a.FrameHeight));frame.Freeze();return (BitmapSource)frame; }).ToArray();
        }
    }
    public void Play(string name) => timing.Play(name);
    public void Update(double dt) => timing.Update(dt);
    public BitmapSource Frame => cache[Current][timing.FrameIndex];
    public BitmapSource GetFrame(string name,int index) => cache[name][index%cache[name].Length];
}
