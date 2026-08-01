import re

dll_path = "/Users/rafael/.nuget/packages/flunt/2.0.5/lib/netstandard2.0/flunt.dll"

with open(dll_path, "rb") as f:
    data = f.read()

# Find all ASCII strings of length 4 to 40
strings = re.findall(b"[a-zA-Z]{4,40}", data)
unique_strings = sorted(list(set(s.decode("ascii") for s in strings)))

# Filter strings containing "Len" or "Length" or "Max" or "Min"
filtered = [s for s in unique_strings if any(x in s.lower() for x in ["len", "length", "max", "min"])]
print("Filtered strings:")
for s in sorted(filtered):
    print(s)
