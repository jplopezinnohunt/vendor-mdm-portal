import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';
import { PublicClientApplication, EventType, AuthenticationResult } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { msalConfig } from "./authConfig";

const msalInstance = new PublicClientApplication(msalConfig);

// Handle redirect callbacks from Azure AD
msalInstance.initialize().then(() => {
  // Handle the redirect response (if any) after MSAL is initialized
  msalInstance.handleRedirectPromise().then((response: AuthenticationResult | null) => {
    if (response) {
      console.log('[MSAL] Redirect login successful');
      // Set the active account if we got a response
      msalInstance.setActiveAccount(response.account);
    }
  }).catch((error) => {
    console.error('[MSAL] Redirect error:', error);
  });

  // Set active account on page load if accounts exist
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length > 0) {
    msalInstance.setActiveAccount(accounts[0]);
  }

  // Listen for login events
  msalInstance.addEventCallback((event) => {
    if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
      const payload = event.payload as AuthenticationResult;
      msalInstance.setActiveAccount(payload.account);
    }
  });

  const rootElement = document.getElementById('root');
  if (!rootElement) {
    throw new Error("Could not find root element to mount to");
  }

  const root = ReactDOM.createRoot(rootElement);

  root.render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    </React.StrictMode>
  );
});