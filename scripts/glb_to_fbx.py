"""Convert every .glb in Assets/Resources/SkyBox to .fbx in Assets/Resources/SkyBox/Converted."""
import bpy, os, glob, sys

SRC = r"C:\Users\tomwe\TandaHackathon\QuickFlick\Assets\Resources\SkyBox"
DST = os.path.join(SRC, "Converted")
os.makedirs(DST, exist_ok=True)

for glb in glob.glob(os.path.join(SRC, "*.glb")):
    name = os.path.splitext(os.path.basename(glb))[0]
    out = os.path.join(DST, name + ".fbx")
    print(f"=== {name} ===", flush=True)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    try:
        bpy.ops.import_scene.gltf(filepath=glb)
    except Exception as e:
        print(f"IMPORT_FAIL {name}: {e}", flush=True)
        continue

    try:
        bpy.ops.export_scene.fbx(
            filepath=out,
            path_mode='COPY',
            embed_textures=True,
            bake_space_transform=True,
            object_types={'ARMATURE', 'MESH', 'EMPTY'},
        )
        print(f"OK {name} -> {out}", flush=True)
    except Exception as e:
        print(f"EXPORT_FAIL {name}: {e}", flush=True)
