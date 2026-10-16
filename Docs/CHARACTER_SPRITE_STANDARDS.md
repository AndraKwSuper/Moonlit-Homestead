# STANDAR ASET SPRITE & IMPORT KARAKTER (ELIAN) - MOONLIT HOMESTEAD

Semua sprite animasi aksi/gerakan Elian (Idle, Walk, Hoe, Water, Plant, Harvest, dll) WAJIB mengikuti standar teknis berikut agar posisi karakter stabil (100% konsisten, tidak bergeser, dan tidak melompat ke tanah/udara saat berganti animasi).

---

### 1. Standar Canvas & Gambar (Artist / Export)
- **Dimensi Kanvas:** 512 × 512 piksel (Square, rasio 1:1).
- **Baseline Telapak Kaki (Ground Line):**
  - Telapak kaki bagian bawah WAJIB menyentuh garis **32 piksel dari batas bawah kanvas** ($Y = 32$ px dari bawah, atau baris $480$ pada koordinat top-down).
  - Badan karakter diposisikan simetris di tengah kanvas ($X = 256$ px).
- **Format:** PNG 32-bit dengan background transparan.

---

### 2. Standar Unity Sprite Importer (Inspector / .meta)
Setiap sprite karakter baru wajib memiliki konfigurasi import berikut:
- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Single
- **Pixels Per Unit (PPU):** `280` *(Wajib 280 agar proporsi tubuh 1:1 identik di scene)*
- **Pivot:** `Custom`
  - **Pivot X:** `0.5`
  - **Pivot Y:** `0.0625` *(Diperoleh dari rumus: 32 px / 512 px)*
  *(Di file .meta: `alignment: 7`, `spritePivot: {x: 0.5, y: 0.0625}`)*
- **Filter Mode:** `Bilinear`
- **Compression:** Sesuai preset default proyek (Normal Quality)

> ⚠️ **PENTING:** JANGAN gunakan Pivot `Center (0.5, 0.5)`. Menggunakan Center akan menyebabkan karakter ambles/terjun bebas ke bawah tanah sejauh ~0.8 unit dunia saat animasi aksi diputar.

---

### 3. Standar Pembuatan Animation Clip (.anim)
- **Target Properti:** HANYA menganimasikan `SpriteRenderer.sprite`.
- **DILARANG** menambahkan keyframe pada `Transform.position`, `Transform.rotation`, atau `Transform.localScale` di dalam clip.
- **Sampling Rate:** `10` - `12` FPS (disesuaikan dengan tempo aksi).
- **Loop Time:**
  - `True` untuk animasi looping (Idle, Walk).
  - `False` untuk animasi aksi satu kali (Hoeing, Watering, Reaping).

---

### 4. Standar Eksekusi di Kode (Gameplay Logic)
- Setiap kali animasi aksi satu kali (one-shot action) seperti mencangkul dipicu:
  1. Panggil trigger animator: `animator.SetTrigger("TriggerHoe");`
  2. Kunci gerakan horizontal: `playerController.LockMovement(durasi);`
  3. Nol-kan momentum fisika seketika: `rb.velocity = new Vector2(0f, rb.velocity.y);`
