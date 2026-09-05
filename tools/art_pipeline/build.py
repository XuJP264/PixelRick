"""Deterministic pixel normalization and cutout-rig animation. No API calls.
Generated identity masters are immutable; all runtime art is rebuilt here.
"""
from pathlib import Path
import json, math, argparse
from PIL import Image, ImageDraw, ImageFont
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'Assets/Character/Rick'
REVIEW = ROOT / 'ArtReview'
N = Image.Resampling.NEAREST
PALETTE = ['#101b2b','#293746','#4b708b','#588eb4','#78bff0','#a3d8f3',
           '#f2f5ef','#c5d0d2','#90a2ae','#e8c7b1','#cba38b','#ad8775',
           '#00cddd','#0098ae','#006d7a','#755236','#523b2a','#342c27',
           '#ebc72c','#a6dc40','#659828','#345423','#ffffff','#ef7970']
RGB = np.array([tuple(bytes.fromhex(c[1:])) for c in PALETTE],dtype=np.int32)

def blank(): return Image.new('RGBA',(128,128))

def quantize(im):
    a=np.array(im.convert('RGBA')); rgb=a[:,:,:3].astype(np.int32)
    idx=((rgb[:,:,None,:]-RGB[None,None,:,:])**2).sum(axis=3).argmin(axis=2)
    a[:,:,:3]=RGB[idx]; a[:,:,3]=np.where(a[:,:,3]>=128,255,0)
    a[a[:,:,3]==0]=0
    return Image.fromarray(a)

def extract(path, columns, heights=None):
    im=Image.open(path).convert('RGBA'); a=np.array(im)
    # Chroma key includes lightly contaminated magenta edge pixels.
    key=(a[:,:,0]>140)&(a[:,:,2]>130)&(a[:,:,1]<110)
    a[key]=0
    im=Image.fromarray(a); result=[]
    for i in range(columns):
        cell=im.crop((i*im.width//columns,0,(i+1)*im.width//columns,im.height))
        mask=np.array(cell)[:,:,3]>0
        # Remove disconnected specks; retain all substantial body components.
        seen=np.zeros(mask.shape,bool);components=[]
        for y,x in zip(*np.where(mask)):
            if seen[y,x]: continue
            stack=[(y,x)]; seen[y,x]=True; component=[]
            while stack:
                yy,xx=stack.pop(); component.append((yy,xx))
                for ny,nx in ((yy-1,xx),(yy+1,xx),(yy,xx-1),(yy,xx+1)):
                    if 0<=ny<mask.shape[0] and 0<=nx<mask.shape[1] and mask[ny,nx] and not seen[ny,nx]:
                        seen[ny,nx]=True; stack.append((ny,nx))
            components.append(component)
        mask[:]=False
        for yy,xx in max(components,key=len):mask[yy,xx]=True
        ca=np.array(cell); ca[~mask]=0; cell=Image.fromarray(ca)
        box=cell.getbbox()
        if not box: raise ValueError('Empty source cell')
        cell=cell.crop(box); height=(heights or [92]*columns)[i]
        cell=quantize(cell.resize((round(cell.width*height/cell.height),height),N))
        out=blank(); out.alpha_composite(cell,(64-cell.width//2,120-cell.height)); result.append(out)
    return result

def shift(im,x=0,y=0):
    out=blank(); out.alpha_composite(im,(int(x),int(y))); return out

def squash(im,amount=0,dx=0):
    box=im.getbbox(); cut=im.crop(box); cut=cut.resize((cut.width,cut.height-amount),N)
    out=blank(); out.alpha_composite(cut,(box[0]+dx,120-cut.height)); return out

def rig_walk(im,i):
    out=blank(); phase=math.sin(i*math.tau/8); bob=-int(i%4==1)
    # Independent lower-leg pieces swing under the same unmodified head/coat.
    upper=im.crop((0,0,128,108)); out.alpha_composite(upper,(0,bob))
    legs=im.crop((0,108,128,128))
    out.alpha_composite(legs,(round(-phase*4),108-abs(round(phase*2))))
    out.alpha_composite(legs,(round(phase*4),108))
    return out

def blink(im,closed):
    if not closed:return im.copy()
    out=im.copy();d=ImageDraw.Draw(out)
    # Canonical face rig coordinates, reviewed at 8x magnification.
    for left,right in [(49,63),(64,78)]:
        d.rectangle((left,61,right,74),fill=PALETTE[9])
        d.line((left+1,67,left+3,68,right-3,68,right-1,67),fill=PALETTE[0],width=1)
    return out

def make_prototype(bases):
    front,side,grab=bases
    return {
      'idle':([squash(front,a) for a in [0,0,1,1,0,0]],3,True),
      'walk':([rig_walk(side,i) for i in range(8)],9,True),
      'grabbed':([shift(grab,x,-2) for x in [0,-1,0,1]],7,True),
      'falling':([shift(grab,0,y) for y in [-2,-1,0,-1]],8,True),
      'landing':([squash(front,a) for a in [4,6,4,2,0,0]],12,False)}

def seated(front,depth=12):
    # Articulated seated pose: lower the rigid head + torso and bend the legs.
    out=blank(); upper=front.crop((0,0,128,103));out.alpha_composite(upper,(0,depth))
    d=ImageDraw.Draw(out)
    d.polygon([(57,109),(70,109),(78,115),(78,119),(65,119),(62,114),(53,119),(45,119),(45,115)],fill=PALETTE[0])
    d.rectangle((54,111,69,114),fill=PALETTE[15]);d.rectangle((48,115,59,117),fill=PALETTE[15]);d.rectangle((67,115,75,117),fill=PALETTE[15])
    d.rectangle((44,117,55,119),fill=PALETTE[1]);d.rectangle((70,117,81,119),fill=PALETTE[1])
    return quantize(out)

def smirk(front):
    out=front.copy(); d=ImageDraw.Draw(out)
    d.line((59,79,67,79,70,77),fill=PALETTE[0],width=1)
    return out

def full_character(bases):
    front,side,grab=bases; closed=blink(front,True); sit=seated(front); asleep=seated(closed)
    animations=make_prototype(bases)
    animations.update({
      'blink':([front,closed,closed,front],9,False),
      'look_around':([front,shift(front,-1),side,side,front,front],4,False),
      'sit':([front,squash(front,2),squash(front,5),seated(front,8),sit,sit],7,False),
      'sitting_idle':([sit,sit,squash(sit,1),sit],3,True),
      'sleep':([asleep,asleep,squash(asleep,1),squash(asleep,1),asleep,asleep],2,True),
      'wake_up':([asleep,sit,seated(front,8),squash(front,5),squash(front,2),front],6,False),
      'angry':([shift(grab,x) for x in [0,-1,1,-2,2,-1,1,0]],10,False),
      'dizzy':([shift(closed,x,y) for x,y in [(0,0),(-2,-1),(-3,0),(0,-2),(3,0),(2,-1)]],6,False),
      'poked':([front,shift(grab,-2,-1),squash(grab,2),shift(front,1),front,front],10,False),
      'smirk':([front,smirk(front),squash(smirk(front),1),smirk(front),front,front],5,False),
      'portal_enter':([squash(side,a) for a in [0,1,0,1,2,3,3,3]],7,False),
      'portal_exit':([squash(side,a) for a in [3,3,3,2,1,0,1,0]],7,False)})
    return animations

def make_effects():
    path=ROOT/'Assets/Effects/reference/effects.png'
    if not path.exists():return {}
    im=Image.open(path).convert('RGBA');a=np.array(im);a[(a[:,:,0]>140)&(a[:,:,2]>130)&(a[:,:,1]<110)]=0;im=Image.fromarray(a)
    names=['portal','zzz','anger','question','exclamation','dust','impact','sparkle'];result={}
    for idx,name in enumerate(names):
        x=idx%4;y=idx//4;cell=im.crop((x*im.width//4,y*im.height//2,(x+1)*im.width//4,(y+1)*im.height//2));cell=cell.crop(cell.getbbox())
        target={'portal':(65,103),'zzz':(25,24),'anger':(19,19),'question':(15,24),'exclamation':(12,24),'dust':(55,18),'impact':(25,25),'sparkle':(20,20)}[name]
        cell=quantize(cell.resize(target,N));frames=[]
        for i in range(8 if name=='portal' else 4):
            out=blank();cut=cell
            if name=='portal':
                # Pixel palette cycling animates the swirl without resampling texture.
                ca=np.array(cut);green=(ca[:,:,1]>ca[:,:,0]*1.05)&(ca[:,:,1]>ca[:,:,2]*1.1)&(ca[:,:,3]>0)
                ca[green,:3]=RGB[[19,20,21,19,20,19,21,20][i]];cut=Image.fromarray(ca)
                width=[23,39,57,65,65,57,39,23][i];cut=cut.resize((width,103),N);pos=(64-width//2,17)
            elif name=='dust':
                cut=cut.resize((43+i*5,18-i*2),N);pos=(64-cut.width//2,120-cut.height)
            else:pos=(86,8-(i%2)*2)
            out.alpha_composite(cut,pos);frames.append(out)
        result[name]=(frames,7 if name=='portal' else 4,True)
    return result

def demo(animations,effects):
    frames=[]
    font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18) if Path('C:/Windows/Fonts/segoeui.ttf').exists() else ImageFont.load_default(size=18)
    for name in ['idle','walk','blink','look_around','grabbed','falling','landing','angry','poked','smirk','sit','sleep','wake_up','portal_enter','portal_exit']:
        sprites,fps,_=animations[name]
        for tick in range(18):
            canvas=Image.new('RGB',(720,360),'#121c2c');d=ImageDraw.Draw(canvas)
            d.text((28,22),'PIXELRICK',font=font,fill='#a3d8f3');d.text((28,52),'A tiny genius. A little desktop chaos.',font=font,fill='#90a2ae')
            d.rectangle((0,324,720,359),fill='#202e43');d.text((28,331),name.replace('_',' ').title(),font=font,fill='#c5d0d2')
            sprite=sprites[int(tick*fps/10)%len(sprites)].resize((256,256),N)
            x=232+int(tick*4) if name=='walk' else 232
            if name.startswith('portal') and 'portal' in effects:
                portal=effects['portal'][0][tick%8].resize((256,256),N);canvas.paste(portal,(x,84),portal)
                if (name=='portal_enter' and tick>11) or(name=='portal_exit' and tick<5):sprite=Image.new('RGBA',sprite.size)
            canvas.paste(sprite,(x,84),sprite)
            fx={'sleep':'zzz','angry':'anger','landing':'dust','poked':'impact','look_around':'question'}.get(name)
            if fx in effects:
                effect=effects[fx][0][tick%4].resize((256,256),N);canvas.paste(effect,(x,84),effect)
            frames.append(canvas)
    frames[0].save(REVIEW/'full_behavior_demo.gif',save_all=True,append_images=frames[1:],duration=100,loop=0,optimize=True)
    frames[0].save(REVIEW/'preview.png')

def save_animation(name,frames,fps,loop,folder,metadata):
    sheet=Image.new('RGBA',(128*len(frames),128))
    for i,frame in enumerate(frames): sheet.alpha_composite(frame,(128*i,0))
    folder.mkdir(parents=True,exist_ok=True); sheet.save(folder/f'{name}.png',optimize=True)
    metadata[name]={'file':str((folder/f'{name}.png').relative_to(ROOT/'Assets')).replace('\\','/'),
       'frameWidth':128,'frameHeight':128,'frames':len(frames),'fps':fps,'loop':loop,'anchorX':64,'anchorY':120}
    previews=[]
    for frame in frames:
        bg=Image.new('RGB',(128,128),'#202b3c');bg.paste(frame,mask=frame.getchannel('A'));previews.append(bg.resize((256,256),N))
    previews[0].save(REVIEW/f'{name}.gif',save_all=True,append_images=previews[1:],duration=round(1000/fps),loop=0,disposal=2)

def validate(metadata):
    report={'passed':True,'palette':PALETTE,'sprites':{}}
    for name,m in metadata.items():
        path=ROOT/'Assets'/m['file'];errors=[];boxes=[]
        with Image.open(path) as im:
            if im.format!='PNG' or im.mode!='RGBA':errors.append('PNG RGBA required')
            if im.size!=(128*m['frames'],128):errors.append('Dimensions')
            for i in range(m['frames']):
                frame=im.crop((128*i,0,128*(i+1),128));box=frame.getbbox();boxes.append(box)
                if not box:errors.append(f'Empty frame {i}');continue
                if box[0]<2 or box[1]<2 or box[2]>126 or box[3]>122:errors.append(f'Clipping frame {i}')
                colors=frame.getcolors(16384)
                if any(c[3] not in (0,255) for _,c in colors):errors.append('Partial alpha')
                if any(c[3] and tuple(c[:3]) not in [tuple(v) for v in RGB] for _,c in colors):errors.append('Palette')
                if 'Character' in m['file']:
                    if not 70<=box[3]-box[1]<=98 or not 35<=box[2]-box[0]<=95:errors.append(f'Inconsistent body dimensions {i}')
                    if abs((box[0]+box[2])/2-64)>14: errors.append(f'Anchor X {i}')
                    if not 112<=box[3]<=120:errors.append(f'Anchor Y {i}')
        report['sprites'][name]={'passed':not errors,'errors':errors,'frames':m['frames'],'bounds':boxes}
        report['passed'] &= not errors
    (ROOT/'art_report.json').write_text(json.dumps(report,indent=2)+'\n')
    if not report['passed']:raise ValueError(json.dumps(report,indent=2))
    return report

def contact(animations):
    out=Image.new('RGB',(128*10,160*len(animations)), '#202b3c'); d=ImageDraw.Draw(out)
    for row,(name,(frames,fps,loop)) in enumerate(animations.items()):
        d.text((16,row*160+8),f'{name.upper()}   {len(frames)} frames / {fps} fps',fill='#b7d9ed')
        for i,f in enumerate(frames):out.paste(f,(i*128,row*160+27),f)
    out.save(REVIEW/'all_animations_contact_sheet.png')

def main():
    REVIEW.mkdir(exist_ok=True);(ART/'Sprites').mkdir(exist_ok=True)
    bases=extract(ART/'reference/base_poses.png',3)
    for name,im in zip(['front','right','grabbed'],bases):im.save(ART/'reference'/f'{name}.png')
    ref=Image.new('RGB',(768,384),'#202b3c')
    for i,im in enumerate([bases[0],bases[1].transpose(Image.Transpose.FLIP_LEFT_RIGHT),bases[1]]):ref.paste(im.resize((256,256),N),(i*256,0),im.resize((256,256),N))
    d=ImageDraw.Draw(ref)
    for i,c in enumerate(PALETTE):d.rectangle((24+i*30,302,47+i*30,325),fill=c)
    d.text((24,350),'PIXELRICK / CANONICAL PALETTE + FRONT / LEFT / RIGHT',fill='#a3d8f3');ref.save(REVIEW/'character_reference.png')
    animations=full_character(bases); effects=make_effects(); metadata={}
    for name,(frames,fps,loop) in animations.items():save_animation(name,frames,fps,loop,ART/'Sprites',metadata)
    for name,(frames,fps,loop) in effects.items():save_animation(name,frames,fps,loop,ROOT/'Assets/Effects',metadata)
    (ART/'animations.json').write_text(json.dumps(metadata,indent=2)+'\n');contact(animations);validate(metadata)
    demo(animations,effects)
    icon=bases[0].crop((28,27,101,85));square=Image.new('RGBA',(80,80));square.alpha_composite(icon,((80-icon.width)//2,(80-icon.height)//2))
    sizes=[(16,16),(32,32),(48,48),(64,64),(128,128),(256,256)]
    square.resize((256,256),N).save(ROOT/'Assets/app.ico',sizes=sizes,append_images=[square.resize(size,N) for size in sizes])
    print(f'PASS: {len(metadata)} animations, {sum(m["frames"] for m in metadata.values())} frames')

if __name__=='__main__':main()
