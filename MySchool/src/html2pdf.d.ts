/* minimal ambient typings for html2pdf.js */
declare module 'html2pdf.js' {
    /* ✅  make it *exported* so you can import it */
    export interface Html2PdfOptions {
      margin?: number | number[];
      filename?: string;
      image?: { type?: string; quality?: number };
      html2canvas?: { scale?: number; useCORS?: boolean };
      jsPDF?: {
        unit?: string;
        format?: string | [number, number];
        orientation?: 'portrait' | 'landscape';
      };
    }
  
    /* CommonJS export = style */
    function html2pdf(): {
      set:  (opt: Html2PdfOptions) => any;
      from: (elm: HTMLElement) => any;
      save: () => void;
    };
  
    export = html2pdf;
  }
  