# Mini Digital Product Passport (Mini DPP)

Technical assessment project for ESG Next. The system imports user-provided product CSV data, stores it in SQLite, and exposes product APIs plus public HTML/JSON-LD Digital Product Passports and QR codes, alongside a modern, responsive web application.

## Backend technology stack

- ASP.NET Core Web API (controllers) on .NET 10
- Entity Framework Core with SQLite
- CsvHelper
- QRCoder
- xUnit

## Frontend technology stack

- React 19 with TypeScript (strict mode)
- Vite 8
- Tailwind CSS v4
- TanStack Query
- Lucide React

## Prerequisites

- .NET SDK 10
- Node.js 22 or later

## Database

The API applies EF Core migrations on startup. To create or update the SQLite database manually:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update --project backend/MiniDpp.Api/MiniDpp.Api.csproj
```

To add a new migration after model changes:

```bash
dotnet ef migrations add <Name> --project backend/MiniDpp.Api/MiniDpp.Api.csproj --output-dir Data/Migrations
```

The SQLite file is `backend/MiniDpp.Api/minidpp.db` when the API is started from that project directory.

## Run the backend

```bash
cd backend/MiniDpp.Api
dotnet restore
dotnet run --launch-profile http
```

The API listens on `http://localhost:5141`.

- Health: `GET http://localhost:5141/api/health`
- Swagger UI (Development): `http://localhost:5141/swagger`

QR codes use `PublicApp:BaseUrl` from `appsettings.json`. Change that value when hosting on another origin; do not hardcode a development port in code.

## CSV import

Users can upload their own product datasets directly through the frontend web interface or via the HTTP API.

The CSV file must be UTF-8 encoded, comma-separated, use `.` as the decimal separator, and keep GTIN values as 14-character numeric strings (including leading zeros).

### Expected columns

```csv
gtin,model_adi,batch_no,elyaf_pamuk_yuzde,elyaf_elastan_yuzde,uretim_yeri,karbon_kg_co2e
```

### Validation & import rules

1. **Independent row validation**: Each row is evaluated on its own. Valid rows are successfully persisted into the database even if other rows in the file fail validation.
2. **Detailed error reporting**: Any invalid row produces an error with its 1-based CSV line number (line 1 is the header row) and specific failure reasons.
3. **Fields**:
   - `gtin`: Exactly 14 digits with a valid GS1 check digit.
   - `model_adi`, `batch_no`, `uretim_yeri`: Required non-empty strings.
   - `elyaf_pamuk_yuzde`, `elyaf_elastan_yuzde`: Non-negative decimals (0–100) whose sum must equal exactly 100.
   - `karbon_kg_co2e`: Non-negative decimal value.
4. **Duplicate GTIN handling**: GTIN is the product primary key. The first valid occurrence of a GTIN is saved. Later rows in the same file, or subsequent uploads with an already existing GTIN, are rejected with `A product with this GTIN already exists.` Existing records are never overwritten.
5. **File limit**: Uploads are restricted to 2 MB and must be `.csv` files.

### Uploading via HTTP API (curl)

```bash
curl -sS -X POST http://localhost:5141/api/import \
  -F "file=@/path/to/your_products.csv;type=text/csv"
```

## Example API requests

```bash
curl -sS http://localhost:5141/api/health
curl -sS http://localhost:5141/api/products
curl -sS http://localhost:5141/api/products/08690000000012
curl -sS -X DELETE http://localhost:5141/api/products/08690000000012
```

HTML DPP:

`http://localhost:5141/dpp/08690000000012`

JSON-LD DPP:

`http://localhost:5141/dpp/08690000000012.jsonld`

QR PNG (encodes the public HTML DPP URL):

`http://localhost:5141/api/products/08690000000012/qr`

```bash
curl -sS -o passport.png http://localhost:5141/api/products/08690000000012/qr
```

## Visibility rule

Carbon (`karbon_kg_co2e`) is omitted from public HTML and JSON-LD by default. It is not hidden with CSS; it is not serialized into those public responses.

Add `?rol=yetkili` to include carbon in both representations. This is a demonstration switch only; there is no login or JWT.

- `/dpp/{gtin}` — no carbon
- `/dpp/{gtin}?rol=yetkili` — carbon visible
- `/dpp/{gtin}.jsonld` — no carbon
- `/dpp/{gtin}.jsonld?rol=yetkili` — carbon visible

`GET /api/products` returns carbon for internal/frontend catalog use.

## Frontend application

The frontend application provides an end-to-end workflow:

**Upload CSV → View Import Results → Browse Products → Open Digital Product Passport**

1. **Upload CSV**: Drag and drop or browse for a local `.csv` file with instant file size and format validation.
2. **View Import Results**: Real-time feedback displaying total rows processed, saved rows, failed rows, and line-by-line validation errors.
3. **Browse Products**: Interactive catalog of all registered products, displaying model names, batch numbers, production locations, material composition, and carbon footprint.
4. **Product Lookup & Passport**: Enter any 14-digit GTIN to inspect its Digital Product Passport, complete with GS1 check digit validation, live QR code display, authorized ESG view toggle, and direct links to machine-readable JSON-LD and HTML representations.

### Run the frontend

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

The Vite dev server runs at `http://localhost:5173` and communicates with the backend at `http://localhost:5141` via `VITE_API_BASE_URL`.

## Tests

```bash
dotnet test
```

Unit and integration tests run against self-contained in-memory / temporary SQLite instances and do not touch the development database.
