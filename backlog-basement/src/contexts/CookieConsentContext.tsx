import { createContext, useContext, useState, useCallback, ReactNode } from 'react';

export interface CookieConsent {
  essential: boolean;
  preferences: boolean;
  timestamp: string;
}

interface CookieConsentContextType {
  consent: CookieConsent | null;
  hasConsented: boolean;
  updateConsent: (consent: Omit<CookieConsent, 'essential' | 'timestamp'>) => void;
  acceptAll: () => void;
}

const STORAGE_KEY = 'cookie-consent';

const CookieConsentContext = createContext<CookieConsentContextType | undefined>(undefined);

function loadConsent(): CookieConsent | null {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      return JSON.parse(stored);
    }
  } catch {
    // Ignore parse errors
  }
  return null;
}

function saveConsent(consent: CookieConsent): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(consent));
}

export function CookieConsentProvider({ children }: { children: ReactNode }) {
  const [consent, setConsent] = useState<CookieConsent | null>(loadConsent);

  const updateConsent = useCallback((partial: Omit<CookieConsent, 'essential' | 'timestamp'>) => {
    const newConsent: CookieConsent = {
      essential: true,
      preferences: partial.preferences,
      timestamp: new Date().toISOString(),
    };
    saveConsent(newConsent);
    setConsent(newConsent);
  }, []);

  const acceptAll = useCallback(() => {
    updateConsent({ preferences: true });
  }, [updateConsent]);

  return (
    <CookieConsentContext.Provider value={{ consent, hasConsented: consent !== null, updateConsent, acceptAll }}>
      {children}
    </CookieConsentContext.Provider>
  );
}

export function useCookieConsent(): CookieConsentContextType {
  const context = useContext(CookieConsentContext);
  if (context === undefined) {
    throw new Error('useCookieConsent must be used within a CookieConsentProvider');
  }
  return context;
}
