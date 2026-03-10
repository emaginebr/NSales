import React from 'react';
import ReactDOM from 'react-dom/client';
//import './bootstrap-bwhale.css';
import './bootstrap.css';
import './editmode.css';
import 'react-loading-skeleton/dist/skeleton.css';
import App from './App';
import { BrowserRouter } from "react-router-dom";
import ScrollToTop from './Components/ScrollToTop';
import './i18n'; // Import i18next configuration

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <BrowserRouter basename={process.env.REACT_APP_SITE_BASENAME}>
      <ScrollToTop />
      <React.Suspense fallback="loading...">
        <App />
      </React.Suspense>
    </BrowserRouter>
  </React.StrictMode>
);
