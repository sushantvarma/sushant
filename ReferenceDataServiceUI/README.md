# Reference Data Service UI

A React + Vite frontend application for the Reference Data Service POC. This UI allows users to load reference data from Snowflake through an on-demand API endpoint and view it in an interactive dropdown.

## Features

✅ **Load Reference Data** - Button to trigger API calls  
✅ **Dynamic Dropdown** - Populates with data from API response  
✅ **Loading Spinner** - Visual feedback during API calls  
✅ **Performance Metrics** - Displays API execution time  
✅ **Error Handling** - User-friendly error messages  
✅ **Correlation ID** - Tracks requests across frontend and backend  
✅ **Responsive Design** - Works on desktop, tablet, and mobile  
✅ **Data Display** - JSON viewer for selected items  

## Prerequisites

- **Node.js** 16+ ([Download](https://nodejs.org))
- **npm** or **yarn** package manager
- **Reference Data Service** running on `https://localhost:5001`

## Project Structure

```
ReferenceDataServiceUI/
├── index.html                    # HTML entry point
├── package.json                  # Dependencies and scripts
├── vite.config.js               # Vite configuration
├── .gitignore                   # Git ignore rules
├── .env.example                 # Environment variables template
├── README.md                    # This file
├── public/                      # Static assets
└── src/
    ├── main.jsx                 # React entry point
    ├── index.css                # Global styles
    ├── App.jsx                  # Main App component
    ├── App.css                  # App component styles
    └── components/
        ├── LoadingSpinner.jsx   # Loading spinner component
        └── LoadingSpinner.css   # Spinner styles
```

## Installation

### Step 1: Navigate to Project Directory

```bash
cd c:\Code\ReferenceDataServiceUI
```

### Step 2: Install Dependencies

```bash
npm install
```

This installs:
- React 18.2.0
- React DOM 18.2.0
- Axios (HTTP client)
- Vite (build tool)
- Vite React plugin

## Running the Application

### Development Mode

```bash
npm run dev
```

**Default URL**: `http://localhost:5173`

The app will automatically open in your default browser.

### Build for Production

```bash
npm run build
```

Creates optimized production build in `dist/` folder.

### Preview Production Build

```bash
npm run preview
```

Serves the production build locally for testing.

## Usage

### Loading Reference Data

1. **Start the Backend**  
   Ensure the Reference Data Service is running:
   ```bash
   cd c:\Code\ReferenceDataService
   dotnet run
   ```

2. **Open the UI**  
   Navigate to `http://localhost:5173` in your browser

3. **Click "Load Reference Data"**  
   - Button will show loading state
   - Loading spinner appears
   - API is called with correlation ID header

4. **View Results**  
   - Data appears in dropdown
   - API execution time is displayed
   - Select items to view full details

### Example Response

```json
{
  "data": [
    { "id": 1, "name": "Product A", "category": "Electronics" },
    { "id": 2, "name": "Product B", "category": "Furniture" },
    { "id": 3, "name": "Product C", "category": "Clothing" }
  ],
  "executionTimeMs": 245
}
```

### Error Handling

If the API call fails:
- Error message is displayed
- User can retry by clicking button again
- Check backend logs for details

## Configuration

### API URL

The default API URL is `https://localhost:5001/api/reference-data`

To change it, edit `src/App.jsx`:

```jsx
const API_URL = 'https://actual-api-url:port/api/reference-data'
```

### Environment Variables (Optional)

Create `.env` file in project root:

```
VITE_API_URL=https://localhost:5001/api/reference-data
```

Then update App.jsx to use it:

```jsx
const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api/reference-data'
```

### SSL Certificate Issues

In development, the app allows self-signed SSL certificates via axios config:

```jsx
httpsAgent: {
  rejectUnauthorized: false
}
```

**Note**: Never use this in production!

## Component Documentation

### App Component (src/App.jsx)

Main component that:
- Manages state for data, loading, and errors
- Handles API calls with correlation ID
- Dynamically extracts dropdown options from various data structures
- Displays loading spinner, error messages, and execution times
- Renders dropdown and data display

**Key Functions**:
- `loadReferenceData()` - Calls API endpoint
- `extractDropdownOptions()` - Parses response data for dropdown

### LoadingSpinner Component (src/components/LoadingSpinner.jsx)

Simple animated spinner component displayed during API calls.

**CSS Animation**: Smooth rotating border animation

## Styling

The app uses modern CSS with:
- **CSS Flexbox** - Layout structure
- **CSS Grid** - Responsive multi-column layouts
- **Gradients** - Visual appeal
- **Animations** - Smooth transitions and spinner
- **Media Queries** - Mobile responsiveness

### Color Scheme

- Primary Blue: `#1e3c72`, `#2a5298`
- Accent Blue: `#4a9eff`
- Success Green: `#4caf50`
- Error Red: `#f66`
- Text: `#333`, `#666`

## Performance Optimization

The UI includes:
- **Axios for HTTP** - Efficient HTTP client
- **React Hooks** - Functional components
- **Vite** - Fast build and dev server
- **Lazy Loading** - CSS and JS are optimized

## Troubleshooting

### Issue: "Cannot connect to API"

**Solution**:
1. Verify backend is running: `dotnet run` in ReferenceDataService
2. Check API is accessible: `https://localhost:5001/swagger`
3. Verify firewall isn't blocking port 5001
4. Check API URL in App.jsx is correct

### Issue: "CORS Error"

**Solution**:
- Backend needs CORS enabled
- Add to `Program.cs`:
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowFrontend", policy =>
      {
          policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
      });
  });
  ```

### Issue: "Loading spinner never stops"

**Solution**:
- Check browser console for errors
- Verify API endpoint URL is correct
- Check backend logs for server errors
- Increase timeout in axios config if needed

### Issue: "npm install fails"

**Solution**:
- Clear npm cache: `npm cache clean --force`
- Delete node_modules: `rmdir /s node_modules`
- Delete package-lock.json
- Run: `npm install --legacy-peer-deps`

## Development Workflow

### Hot Module Replacement (HMR)

Vite automatically reloads the page when you save files. No manual refresh needed!

### Debug Mode

1. Open browser DevTools (F12)
2. Set breakpoints in "Sources" tab
3. Reload page to trigger breakpoints

### Console Logging

API responses are logged to console:
```
console.log('API Response:', response)
```

Check browser console (F12 > Console) for details.

## Building for Production

### Create Production Build

```bash
npm run build
```

Generates optimized files in `dist/` folder.

### Deploy to Server

1. Copy `dist/` folder contents to web server
2. Configure server to serve `index.html` for SPA routing
3. Update API URL for production endpoint
4. Test in production environment

### Example nginx Configuration

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    root /var/www/reference-data-service-ui/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass https://backend-api:5001;
    }
}
```

## Next Steps

### Phase 2: Advanced Features
- Add data filtering and search
- Implement client-side caching
- Add export to CSV/JSON
- Multiple API endpoints
- Real-time data updates

### Phase 3: Backend Integration
- Implement dropdown data in Oracle database
- Add create/update/delete operations
- Authentication and authorization
- Advanced error handling

### Phase 4: Production Ready
- Unit tests with Vitest
- E2E tests with Cypress or Playwright
- Performance monitoring
- Analytics integration
- Accessibility improvements (WCAG)

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| react | 18.2.0 | UI library |
| react-dom | 18.2.0 | React DOM renderer |
| axios | 1.6.2 | HTTP client |
| vite | 5.0.8 | Build tool |
| @vitejs/plugin-react | 4.2.1 | React Vite plugin |

## Resources

- **React Docs**: https://react.dev
- **Vite Docs**: https://vitejs.dev
- **Axios Docs**: https://axios-http.com
- **MDN Web Docs**: https://developer.mozilla.org

## License

Proprietary - Part of Snowflake Connectivity POC

## Support

For issues or questions, contact the development team.

---

**Last Updated**: March 30, 2026
