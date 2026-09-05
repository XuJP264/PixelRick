using System.Windows.Media.Imaging;
using System.Windows;
namespace PixelRick;
public sealed class SpriteAnimator
{
    private readonly CharacterDefinition character;
    private readonly Dictionary<string, BitmapSource[]> cache = new();
    public string Current { get; private set; } = "idle";
    public double Elapsed { get; private set; }
    public bool Finished => !Definition.Loop && Elapsed >= Definition.Frames / Definition.Fps;
    public AnimationDefinition Definition => character.Animations[Current];
    public SpriteAnimator(CharacterDefinition character)
    {
        this.character = character;
        foreach (var (key, a) in character.Animations)
        {
            var bmp=new BitmapImage();bmp.BeginInit();bmp.UriSource=new Uri(System.IO.Path.Combine(character.Root,a.File));bmp.CacheOption=BitmapCacheOption.OnLoad;bmp.EndInit();bmp.Freeze();
            if(bmp.PixelWidth!=a.FrameWidth*a.Frames || bmp.PixelHeight!=a.FrameHeight)throw new System.IO.InvalidDataException(key);
            cache[key]=Enumerable.Range(0,a.Frames).Select(i=> { var frame=new CroppedBitmap(bmp,new Int32Rect(i*a.FrameWidth,0,a.FrameWidth,a.FrameHeight));frame.Freeze();return (BitmapSource)frame; }).ToArray();
        }
    }
    public void Play(string name) { if(!character.Animations.ContainsKey(name))throw new ArgumentException($"Unknown animation: {name}",nameof(name));Current=name;Elapsed=0; }
    public void Update(double dt) => Elapsed += dt;
    public BitmapSource Frame => cache[Current][Definition.Loop ? (int)(Elapsed*Definition.Fps)%Definition.Frames : Math.Min(Definition.Frames-1,(int)(Elapsed*Definition.Fps))];
    public BitmapSource GetFrame(string name,int index) => cache[name][index%cache[name].Length];
}
