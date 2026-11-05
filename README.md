# Mini DPP

Digital Product Passport (DPP) system: CSV import, SQLite storage, product version history, REST APIs, public HTML/JSON-LD passports, and QR codes.

**Stack:** ASP.NET Core (.NET 10), EF Core, SQLite, React 19, TypeScript, Vite, Tailwind CSS v4

## Run

```bash
# Backend (http://localhost:5141)
cd backend/MiniDpp.Api
dotnet run --launch-profile http

# Frontend (http://localhost:5173)
cd frontend
cp .env.example .env
npm install
npm run dev
```

SQLite database file: `backend/MiniDpp.Api/minidpp.db` (migrations apply automatically on startup).  
Swagger UI: http://localhost:5141/swagger

## CSV Import & Version History

Upload a UTF-8, comma-separated `.csv` (max 2 MB):

```csv
gtin,model_adi,batch_no,elyaf_pamuk_yuzde,elyaf_elastan_yuzde,uretim_yeri,karbon_kg_co2e
```

- **New GTIN:** Product is created at version 1.
- **Existing GTIN with changes:** Product is updated, version increments (v1 → v2), and the previous snapshot is archived in version history.
- **Existing GTIN with identical data:** Ignored without creating unnecessary versions.
- **Validation:** Independent per-row validation. Invalid rows report 1-based CSV line numbers; valid rows still save.

```bash
curl -sS -X POST http://localhost:5141/api/import \
  -F "file=@your_products.csv;type=text/csv"
```

## API Endpoints

```bash
# Health & Products
curl http://localhost:5141/api/health
curl http://localhost:5141/api/products
curl http://localhost:5141/api/products/08690000000012
curl http://localhost:5141/api/products/08690000000012/versions
curl -X DELETE http://localhost:5141/api/products/08690000000012
```

| Resource | URL |
|---|---|
| HTML DPP | http://localhost:5141/dpp/08690000000012 |
| JSON-LD | http://localhost:5141/dpp/08690000000012.jsonld |
| QR Code | http://localhost:5141/api/products/08690000000012/qr |

*Note: Carbon data is omitted from public HTML/JSON-LD by default. Append `?rol=yetkili` to display it.*

## Tests

```bash
dotnet test
```
