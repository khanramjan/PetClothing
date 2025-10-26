# Pet Clothing Shop - Frontend

A modern, responsive React application built with Vite, TypeScript, and Tailwind CSS for an e-commerce pet clothing shop.

## Features

- **Modern UI/UX**: Beautiful, responsive design with Tailwind CSS
- **Type-Safe**: Full TypeScript support
- **State Management**: Zustand for efficient state management
- **Routing**: React Router v6 for seamless navigation
- **Forms**: React Hook Form for form validation
- **API Integration**: Axios with automatic token refresh
- **Notifications**: Toast notifications for user feedback
- **Authentication**: JWT-based authentication with refresh tokens
- **Protected Routes**: Role-based access control

## Tech Stack

- React 18
- TypeScript
- Vite
- Tailwind CSS
- Zustand (State Management)
- React Router v6
- Axios
- React Hook Form
- React Toastify
- React Icons

## Prerequisites

- Node.js 18+ 
- npm or yarn

## Getting Started

### 1. Install Dependencies

```powershell
cd frontend
npm install
```

### 2. Environment Setup

The `.env` file is already created with:
```
VITE_API_URL=http://localhost:5000/api
```

Update if your backend is running on a different port.

### 3. Run Development Server

```powershell
npm run dev
```

The application will be available at http://localhost:3000

### 4. Build for Production

```powershell
npm run build
```

### 5. Preview Production Build

```powershell
npm run preview
```

## Project Structure

```
frontend/
├── src/
│   ├── components/          # Reusable components
│   │   ├── admin/          # Admin-specific components
│   │   ├── Layout.tsx      # Main layout wrapper
│   │   ├── Navbar.tsx      # Navigation bar
│   │   ├── Footer.tsx      # Footer component
│   │   └── ProtectedRoute.tsx  # Route protection
│   ├── pages/              # Page components
│   │   ├── admin/          # Admin pages
│   │   ├── Home.tsx
│   │   ├── Products.tsx
│   │   ├── ProductDetail.tsx
│   │   ├── Cart.tsx
│   │   ├── Checkout.tsx
│   │   ├── Login.tsx
│   │   ├── Register.tsx
│   │   ├── Profile.tsx
│   │   └── Orders.tsx
│   ├── store/              # Zustand stores
│   │   ├── authStore.ts    # Authentication state
│   │   └── cartStore.ts    # Cart state
│   ├── lib/                # Utilities
│   │   └── api.ts          # Axios instance with interceptors
│   ├── types/              # TypeScript types
│   │   └── index.ts
│   ├── App.tsx             # Main App component
│   ├── main.tsx           # Application entry point
│   └── index.css          # Global styles
├── public/                 # Static assets
├── index.html
├── package.json
├── tsconfig.json
├── tailwind.config.js
├── vite.config.ts
└── .env
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Lint code with ESLint

## Features Implementation

### Customer Features
- ✅ Product browsing with filtering
- ✅ Product details view
- ✅ Shopping cart management
- ✅ Checkout process
- ✅ Order history
- ✅ User profile management
- ✅ Product reviews

### Admin Features
- ✅ Dashboard with analytics
- ✅ Product management (CRUD)
- ✅ Order management
- ✅ Customer management
- ✅ Inventory tracking

### Authentication
- ✅ User registration
- ✅ User login
- ✅ JWT token refresh
- ✅ Protected routes
- ✅ Role-based access control

## Styling

This project uses Tailwind CSS with custom utility classes defined in `index.css`:

- `.btn` - Base button styles
- `.btn-primary` - Primary button
- `.btn-secondary` - Secondary button
- `.btn-outline` - Outlined button
- `.input` - Input field styles
- `.card` - Card component
- `.badge-*` - Status badges

## API Integration

The application communicates with the backend API using Axios with automatic:
- Token attachment to requests
- Token refresh on 401 errors
- Error handling
- Request/Response interceptors

## State Management

### Auth Store (`authStore.ts`)
- User authentication state
- Login/Register/Logout actions
- Token management

### Cart Store (`cartStore.ts`)
- Shopping cart state
- Add/Update/Remove cart items
- Cart synchronization with backend

## Deployment

### Build

```powershell
npm run build
```

The build output will be in the `dist/` folder.

### Deploy to Vercel

```powershell
npm install -g vercel
vercel
```

### Deploy to Netlify

```powershell
npm install -g netlify-cli
netlify deploy
```

### Environment Variables for Production

Make sure to set:
- `VITE_API_URL` - Your production API URL

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing

1. Follow the existing code structure
2. Use TypeScript for type safety
3. Follow Tailwind CSS conventions
4. Write reusable components
5. Test before committing

## License

MIT
