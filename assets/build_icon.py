from PIL import Image, ImageDraw


SCALE = 2
SIZE = 512 * SCALE


def box(values):
    return tuple(int(value * SCALE) for value in values)


image = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
pixels = image.load()
for y in range(SIZE):
    for x in range(SIZE):
        blend = (x + y) / (2.0 * SIZE)
        pixels[x, y] = (
            int(24 * (1 - blend) + 12 * blend),
            int(25 * (1 - blend) + 13 * blend),
            int(31 * (1 - blend) + 16 * blend),
            255,
        )

mask = Image.new("L", (SIZE, SIZE), 0)
ImageDraw.Draw(mask).rounded_rectangle(box((20, 20, 492, 492)), radius=112 * SCALE, fill=255)
image.putalpha(mask)
draw = ImageDraw.Draw(image)
draw.rounded_rectangle(box((21, 21, 491, 491)), radius=111 * SCALE, outline=(55, 57, 68, 255), width=2 * SCALE)

bars = [
    (116, 218, 140, 294, 0.65),
    (158, 176, 182, 336, 0.78),
    (200, 126, 224, 386, 0.92),
    (244, 94, 268, 418, 1.00),
    (288, 150, 312, 362, 0.90),
    (330, 194, 354, 318, 0.76),
    (372, 228, 396, 284, 0.62),
]
for index, (left, top, right, bottom, opacity) in enumerate(bars):
    amount = index / (len(bars) - 1.0)
    color = (
        int(134 * (1 - amount) + 127 * amount),
        int(120 * (1 - amount) + 115 * amount),
        int(247 * (1 - amount) + 234 * amount),
        int(255 * opacity),
    )
    draw.rounded_rectangle(box((left, top, right, bottom)), radius=12 * SCALE, fill=color)

preview = image.resize((512, 512), Image.Resampling.LANCZOS)
preview.save("assets/Flowtype-icon.png", optimize=True)
preview.save("assets/Flowtype.ico", sizes=[(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (24, 24), (16, 16)])
