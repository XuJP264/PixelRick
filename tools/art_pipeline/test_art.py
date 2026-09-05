import unittest,json,hashlib,sys
from pathlib import Path
from PIL import Image
from build import ROOT,validate,extract,ART
class ArtTests(unittest.TestCase):
    def test_runtime_assets(self):
        metadata=json.loads((ART/'animations.json').read_text());self.assertEqual(len(metadata),25);self.assertTrue(validate(metadata)['passed'])
    def test_required_animation_lengths(self):
        metadata=json.loads((ART/'animations.json').read_text())
        counts={'idle':(4,6),'blink':(3,4),'walk':(6,8),'look_around':(4,6),'sit':(4,6),'sitting_idle':(3,4),'sleep':(4,6),'wake_up':(4,6),'grabbed':(2,4),'falling':(2,4),'landing':(4,6),'angry':(6,8),'dizzy':(4,6),'poked':(4,6),'smirk':(4,6),'portal_enter':(6,10),'portal_exit':(6,10)}
        for key,(lo,hi) in counts.items():self.assertTrue(lo<=metadata[key]['frames']<=hi,key)
    def test_previews_exist(self):
        metadata=json.loads((ART/'animations.json').read_text())
        for key in metadata:
            with Image.open(ROOT/'ArtReview'/f'{key}.gif') as im:self.assertGreater(im.n_frames,1,key)
    def test_sources_and_reproducibility(self):
        generated=extract(ART/'reference/base_poses.png',3)
        for name,frame in zip(['front','right','grabbed'],generated):
            self.assertEqual(frame.tobytes(),Image.open(ART/'reference'/f'{name}.png').tobytes())
    def test_icon(self):
        with Image.open(ROOT/'Assets/app.ico') as icon:self.assertEqual(icon.format,'ICO');self.assertIn((256,256),icon.ico.sizes())
if __name__=='__main__':unittest.main(testRunner=unittest.TextTestRunner(stream=sys.stdout))
