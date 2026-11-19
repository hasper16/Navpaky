"""Generates Navpaky's switch sounds (design D8 of redesign-visual-identity).

Run from the repository root: python tools/build_sounds.py
Standard library only, fixed random seed: two runs give the same files. Writes 44.1 kHz 16-bit
mono WAV files into src/Navpaky.App/Sounds/. The sounds are synthesised here (ADR 0003).
The older key-click, soft-tick and chime files are not touched.
"""
import math
import random
import struct
import wave
from pathlib import Path

RATE = 44100
OUT = Path(__file__).resolve().parents[1] / "src/Navpaky.App/Sounds"


def silence(seconds):
    return [0.0] * int(RATE * seconds)


def mix(into, sound, at=0.0):
    start = int(RATE * at)
    end = start + len(sound)
    if len(into) < end:
        into.extend([0.0] * (end - len(into)))
    for i, value in enumerate(sound):
        into[start + i] += value
    return into


def envelope(i, attack, decay):
    t = i / RATE
    if t < attack:
        return t / attack
    return math.exp(-(t - attack) / decay)


def tone(freq, seconds, peak, decay, glide_to=None, wave_shape="sine"):
    out = []
    phase = 0.0
    n = int(RATE * seconds)
    for i in range(n):
        f = freq if glide_to is None else freq * (glide_to / freq) ** (i / n)
        phase += 2 * math.pi * f / RATE
        if wave_shape == "triangle":
            value = 2 / math.pi * math.asin(math.sin(phase))
        else:
            value = math.sin(phase)
        out.append(peak * envelope(i, 0.004, decay) * value)
    return out


def noise_burst(rng, seconds, peak, centre, width, decay):
    # Band-passed noise: a two-pole resonator around centre.
    r = math.exp(-math.pi * width / RATE)
    c = 2 * r * math.cos(2 * math.pi * centre / RATE)
    y1 = y2 = 0.0
    out = []
    for i in range(int(RATE * seconds)):
        y = (1 - r) * rng.uniform(-1, 1) + c * y1 - r * r * y2
        y2, y1 = y1, y
        out.append(peak * 8 * envelope(i, 0.001, decay) * y)
    return out


def pluck(rng, freq, seconds, peak, damping=0.996):
    # Karplus-Strong plucked string.
    period = int(RATE / freq)
    line = [rng.uniform(-1, 1) for _ in range(period)]
    out = []
    for i in range(int(RATE * seconds)):
        j = i % period
        value = line[j]
        line[j] = damping * 0.5 * (line[j] + line[(j + 1) % period])
        out.append(peak * value)
    return out


def jaw_harp(seconds, peak):
    # A buzzing sawtooth through a resonance that sweeps up, as the mouth changes shape.
    out = []
    y1 = y2 = 0.0
    n = int(RATE * seconds)
    for i in range(n):
        saw = 2 * ((i * 110 / RATE) % 1.0) - 1
        centre = 500 * (1600 / 500) ** min(1.0, i / (RATE * 0.18))
        r = math.exp(-math.pi * 60 / RATE)
        c = 2 * r * math.cos(2 * math.pi * centre / RATE)
        y = (1 - r) * saw + c * y1 - r * r * y2
        y2, y1 = y1, y
        out.append(peak * 6 * envelope(i, 0.005, 0.09) * y)
    return out


def normalise(samples, peak=0.5):
    top = max(abs(v) for v in samples) or 1.0
    return [v * peak / top for v in samples]


def write(name, samples):
    samples = normalise(samples)
    fade = int(RATE * 0.005)
    for i in range(min(fade, len(samples))):
        samples[-1 - i] *= i / fade
    with wave.open(str(OUT / f"{name}.wav"), "wb") as out:
        out.setnchannels(1)
        out.setsampwidth(2)
        out.setframerate(RATE)
        out.writeframes(b"".join(
            struct.pack("<h", int(max(-1.0, min(1.0, v)) * 32767)) for v in samples))


def main():
    rng = random.Random(20261119)
    write("bandura", pluck(rng, 392, 0.6, 0.5))
    spoons = silence(0.16)
    mix(spoons, noise_burst(rng, 0.05, 0.6, 1400, 220, 0.012))
    mix(spoons, noise_burst(rng, 0.05, 0.45, 1700, 220, 0.012), at=0.07)
    write("wooden-spoons", spoons)
    write("drop", tone(900, 0.14, 0.5, 0.04, glide_to=1700))
    glass = mix(tone(2637, 0.5, 0.4, 0.15), tone(3951, 0.35, 0.15, 0.09))
    write("glass", glass)
    cymbals = mix(pluck(rng, 784, 0.5, 0.4, 0.994), pluck(rng, 788, 0.5, 0.4, 0.994), at=0.004)
    write("cymbals", cymbals)
    sopilka = mix(tone(1175, 0.3, 0.5, 0.12), noise_burst(rng, 0.08, 0.05, 2400, 1200, 0.03))
    write("sopilka", sopilka)
    write("jaw-harp", jaw_harp(0.28, 0.5))
    drum = mix(tone(160, 0.2, 0.6, 0.06, glide_to=90),
               noise_burst(rng, 0.14, 0.15, 6000, 3000, 0.04), at=0.01)
    write("frame-drum", drum)
    rising = mix(tone(660, 0.1, 0.5, 0.05, wave_shape="triangle"),
                 tone(990, 0.16, 0.5, 0.07, wave_shape="triangle"), at=0.09)
    falling = mix(tone(990, 0.1, 0.5, 0.05, wave_shape="triangle"),
                  tone(660, 0.16, 0.5, 0.07, wave_shape="triangle"), at=0.09)
    write("navpaky-up", rising)
    write("navpaky-down", falling)


if __name__ == "__main__":
    main()
