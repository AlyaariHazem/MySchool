/**
 * Best-effort prefill for job applications from existing session data only
 * (localStorage userName + JWT claims). No extra API calls.
 */
export interface ApplicantSessionPrefill {
  applicantFirstName?: string;
  applicantLastName?: string;
  email?: string;
  phone?: string;
}

function readJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.');
    if (parts.length < 2) return null;
    let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const pad = base64.length % 4;
    if (pad) base64 += '='.repeat(4 - pad);
    return JSON.parse(atob(base64)) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function readSchoolApplicantPrefill(): ApplicantSessionPrefill {
  const out: ApplicantSessionPrefill = {};
  if (typeof localStorage === 'undefined') return out;

  const un = localStorage.getItem('userName');
  if (un?.trim()) {
    const parts = un.trim().split(/\s+/);
    if (parts.length >= 2) {
      out.applicantFirstName = parts[0];
      out.applicantLastName = parts.slice(1).join(' ');
    } else {
      out.applicantFirstName = parts[0];
    }
  }

  const token = localStorage.getItem('token');
  if (!token) return out;

  const p = readJwtPayload(token);
  if (!p) return out;

  const emailClaim =
    p['email'] ?? p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
  if (typeof emailClaim === 'string' && emailClaim.includes('@')) {
    out.email = emailClaim;
  }

  const phoneClaim =
    p['phone_number'] ??
    p['PhoneNumber'] ??
    p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone'];
  if (typeof phoneClaim === 'string' && phoneClaim.trim()) {
    out.phone = phoneClaim.trim();
  }

  return out;
}
