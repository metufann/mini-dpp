# Mini DPP

ESG Next technical assessment: CSV import, SQLite storage, product APIs, HTML/JSON-LD Digital Product Passports, and QR codes.

**Backend:** ASP.NET Core Web API, EF Core, SQLite, CsvHelper, QRCoder, xUnit  
**Frontend:** React, TypeScript, Vite, Tailwind CSS

Requires .NET SDK 10 and Node.js 22+.

## Run

```bash
# API — http://localhost:5141
cd backend/MiniDpp.Api
dotnet run --launch-profile http

# UI — http://localhost:5173
cd frontend
cp .env.example .env
npm install
npm run dev
```

Migrations run on API startup. SQLite file: `backend/MiniDpp.Api/minidpp.db`.

Swagger: http://localhost:5141/swagger

## CSV import

UTF-8, comma-separated, `.` decimals, 14-digit GTINs (keep leading zeros).

```csv
gtin,model_adi,batch_no,elyaf_pamuk_yuzde,elyaf_elastan_yuzde,uretim_yeri,karbon_kg_co2e
```

Invalid rows are skipped with line numbers (header = line 1); valid rows still save. Duplicate GTINs are rejected, not updated. Max 2 MB.

```bash
curl -sS -X POST http://localhost:5141/api/import \
  -F "file=@sample-data/urunler_duzeltilmis_test.csv;type=text/csv"
```

## API

```bash
curl http://localhost:5141/api/health
curl http://localhost:5141/api/products
curl http://localhost:5141/api/products/08690000000012
curl -X DELETE http://localhost:5141/api/products/08690000000012
```

| | |
|---|---|
| HTML DPP | http://localhost:5141/dpp/08690000000012 |
| JSON-LD | http://localhost:5141/dpp/08690000000012.jsonld |
| QR PNG | http://localhost:5141/api/products/08690000000012/qr |

QR URLs use `PublicApp:BaseUrl` in `appsettings.json`.

Carbon is omitted from public HTML/JSON-LD. Add `?rol=yetkili` to show it. This is not real auth. `GET /api/products` still returns carbon.

## Tests

```bash
dotnet test
```
