import { Component, ViewChild, ViewContainerRef, Type, ElementRef, OnInit, inject, AfterViewInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import Quill from 'quill';

import { StudentMonthResultComponent } from '../../report/student-month-result/student-month-result.component';
import { AccountReportComponent } from '../../report/account-report/account-report.component';
import { ReportTemplateService, ReportTemplateGetDTO } from '../../core/services/report-template.service';

interface ReportOption { 
  label: string; 
  value: Type<any>;
  code: string; // Template code for each report type
}

@Component({
  selector: 'app-allotment',
  templateUrl: './allotment.component.html',
  styleUrls: ['./allotment.component.scss']
})
export class AllotmentComponent implements OnInit, AfterViewInit {

  /* dynamic outlet */
  @ViewChild('reportContainer', { read: ViewContainerRef, static: true })
  reportContainer!: ViewContainerRef;

  /* element we convert to PDF */
  @ViewChild('editorPreview', { static: false })
  editorPreview!: ElementRef<HTMLDivElement>;

  /* Quill editor instance */
  private quillInstance: Quill | null = null;
  
  /* Editor reference */
  @ViewChild('editor') editor: any;

  /* rich‑text form */
  formGroup = new FormGroup({ text: new FormControl('') });
  currentReportHtml!: string; // declare the property

  /* dropdown */
  selectedReportOption: ReportOption | null = null;
  selectedReportCode: string = 'STUDENT_MONTH_RESULT'; // Default template code
  reportOptions: ReportOption[] = [
    { label: 'Student Month Result', value: StudentMonthResultComponent, code: 'STUDENT_MONTH_RESULT' },
    { label: 'Receipt Voucher', value: StudentMonthResultComponent, code: 'RECEIPT_VOUCHER' },
    { label: 'Registration Form', value: StudentMonthResultComponent, code: 'REGISTRATION_FORM' },
    { label: 'Account Report', value: AccountReportComponent, code: 'ACCOUNT_REPORT' }
  ];

  // Services
  private reportTemplateService = inject(ReportTemplateService);
  private toastr = inject(ToastrService);

  // Template state
  currentTemplateId?: number;
  isLoading = false;
  isSaving = false;

  ngOnInit(): void {
    // Set initial selected option
    this.selectedReportOption = this.reportOptions.find(opt => opt.code === this.selectedReportCode) || this.reportOptions[0];
    // Load default template on init
    this.loadTemplate(this.selectedReportCode);
  }

  ngAfterViewInit(): void {
    // Quill instance will be set via onEditorInit
  }

  /**
   * Handle editor initialization - get Quill instance
   */
  onEditorInit(event: any): void {
    // Get Quill instance from the editor
    if (event && event.editor) {
      this.quillInstance = event.editor;
      this.setupQuillPasteHandler();
      
      // Configure Quill to preserve spaces
      this.configureQuillForSpaces();
      
      // If we have pending HTML to load, load it now
      // Check if form control is empty or only has whitespace
      const currentValue = this.formGroup.get('text')?.value || '';
      if (this.currentReportHtml && (!currentValue.trim() || currentValue !== this.currentReportHtml)) {
        // Small delay to ensure editor is fully initialized
        setTimeout(() => {
          this.setEditorContent(this.currentReportHtml);
        }, 150);
      }
    }
  }

  /**
   * Configure Quill to preserve spaces when typing
   * Ensure undo/redo and delete work normally
   */
  private configureQuillForSpaces(): void {
    if (!this.quillInstance) return;
    
    try {
      // Don't set white-space style here - let CSS handle it (user set it to normal)
      // This allows Quill to handle spaces, delete, and undo normally
      
      // Ensure history module is enabled for undo/redo
      const history = this.quillInstance.getModule('history');
      if (history) {
        // History module is already enabled by default in Quill
        // Just make sure it's working
      }
      
      // Don't override keyboard bindings - let Quill handle them normally
      // This ensures delete, undo (Ctrl+Z), and redo work properly
    } catch (e) {
      console.warn('Error configuring Quill:', e);
    }
  }

  /**
   * Setup custom paste handler to preserve HTML structure including tables
   */
  private setupQuillPasteHandler(): void {
    if (!this.quillInstance) return;

    try {
      const root = this.quillInstance.root;
      
      // Add paste event listener to intercept paste operations
      root.addEventListener('paste', (e: ClipboardEvent) => {
        e.preventDefault();
        e.stopPropagation();
        
        // Get HTML from clipboard
        const clipboardData = e.clipboardData;
        if (!clipboardData) return;
        
        let html = clipboardData.getData('text/html');
        const plainText = clipboardData.getData('text/plain');
        
        // If no HTML, use plain text
        if (!html && plainText) {
          html = plainText;
        }
        
        if (html) {
          // Check if HTML contains flexbox (display:flex)
          const hasFlexbox = html.includes('display:flex') || html.includes('display: flex');
          
          if (hasFlexbox) {
            // For flexbox HTML, insert directly to preserve structure
            // Get current selection
            const selection = this.quillInstance!.getSelection(true);
            const range = selection ? selection.index : this.quillInstance!.getLength();
            
            // Get current HTML
            const currentHtml = root.innerHTML;
            let newHtml = '';
            
            // Insert HTML at the correct position
            if (range === 0) {
              newHtml = html + currentHtml;
            } else if (range >= currentHtml.length) {
              newHtml = currentHtml + html;
            } else {
              const beforeHtml = currentHtml.substring(0, range);
              const afterHtml = currentHtml.substring(range);
              newHtml = beforeHtml + html + afterHtml;
            }
            
            // Set HTML directly to preserve flexbox
            root.innerHTML = newHtml;
            this.quillInstance!.update('user');
            
            // Update form control
            this.formGroup.get('text')?.setValue(newHtml, { emitEvent: false });
            this.currentReportHtml = newHtml;
          } else {
            // For regular HTML, use Quill's native paste method
            const selection = this.quillInstance!.getSelection(true);
            const range = selection ? selection.index : this.quillInstance!.getLength();
            
            // If there's selected content, delete it first
            if (selection && selection.length > 0) {
              this.quillInstance!.deleteText(selection.index, selection.length, 'user');
              const newRange = selection.index;
              this.quillInstance!.clipboard.dangerouslyPasteHTML(newRange, html);
            } else {
              this.quillInstance!.clipboard.dangerouslyPasteHTML(range, html);
            }
            
            // Get the updated HTML after Quill processes it
            const updatedHtml = root.innerHTML;
            
            // Update form control
            this.formGroup.get('text')?.setValue(updatedHtml, { emitEvent: false });
            this.currentReportHtml = updatedHtml;
            
            // Set cursor position after pasted content
            setTimeout(() => {
              try {
                // Get the length of pasted content in Quill's format
                const newLength = this.quillInstance!.getLength();
                const newPosition = Math.min(range + newLength, newLength);
                this.quillInstance!.setSelection(newPosition, 0, 'user');
              } catch (e) {
                // If selection fails, set to end
                const length = this.quillInstance!.getLength();
                this.quillInstance!.setSelection(length, 0, 'user');
              }
            }, 10);
          }
        }
      }, true); // Use capture phase
      
      // Don't add custom matchers - they cause HTML to be inserted as plain text
      // Let Quill handle HTML conversion normally using dangerouslyPasteHTML
    } catch (e) {
      console.warn('Could not setup custom paste handler:', e);
    }
  }

  /**
   * Set editor content with HTML using Quill's native method
   * This method preserves spaces and flexbox layouts correctly
   */
  private setEditorContent(htmlContent: string): void {
    if (!this.quillInstance || !htmlContent) return;

    try {
      // Clear editor first - use empty Delta to ensure clean state
      const Delta = (Quill as any).import('delta');
      this.quillInstance.setContents(new Delta(), 'silent');
      
      // Ensure editor is ready
      this.quillInstance.update('silent');
      
      // For HTML with flexbox (display:flex), we need to preserve it directly
      // Quill doesn't support flexbox natively, so we'll set it directly
      const hasFlexbox = htmlContent.includes('display:flex') || htmlContent.includes('display: flex');
      
      if (hasFlexbox) {
        // Set HTML directly to preserve flexbox structure
        // Then update Quill's state
        this.quillInstance.root.innerHTML = htmlContent;
        this.quillInstance.update('user');
        
        // Update form control and current HTML
        this.formGroup.get('text')?.setValue(htmlContent, { emitEvent: false });
        this.currentReportHtml = htmlContent;
        
        // Force a refresh
        setTimeout(() => {
          this.quillInstance!.update();
        }, 50);
      } else {
        // For regular HTML, use Quill's native method
        this.quillInstance.clipboard.dangerouslyPasteHTML(0, htmlContent);
        this.quillInstance.update('user');
        
        // Get the final HTML after Quill processes it
        setTimeout(() => {
          try {
            const finalHtml = this.quillInstance!.root.innerHTML;
            if (finalHtml) {
              this.formGroup.get('text')?.setValue(finalHtml, { emitEvent: false });
              this.currentReportHtml = finalHtml;
            }
            this.quillInstance!.update();
          } catch (e) {
            console.warn('Error getting final HTML:', e);
          }
        }, 150);
      }
      
    } catch (e) {
      console.warn('Error setting editor content:', e);
      // Fallback to form control
      this.formGroup.get('text')?.setValue(htmlContent);
    }
  }


  /**
   * Handle editor text change to preserve HTML content
   * This ensures spaces are preserved when the user types
   * Use debouncing to avoid interfering with normal operations (delete, undo, etc.)
   */
  private textChangeTimeout: any = null;
  
  onEditorTextChange(event: any): void {
    // Clear any pending updates
    if (this.textChangeTimeout) {
      clearTimeout(this.textChangeTimeout);
    }
    
    // Debounce the update to avoid interfering with delete, undo, and normal typing
    // IMPORTANT: Only update currentReportHtml, NOT the form control during typing
    // Updating form control causes cursor to jump to beginning
    this.textChangeTimeout = setTimeout(() => {
      if (this.quillInstance) {
        const html = this.quillInstance.root.innerHTML;
        if (html) {
          // Only update currentReportHtml - this is what we use for saving
          // DO NOT update form control here - it causes cursor to jump
          this.currentReportHtml = html;
        }
      } else if (event && event.htmlValue) {
        // Fallback if Quill instance not available
        this.currentReportHtml = event.htmlValue;
      }
    }, 500); // 500ms debounce - enough time for undo/redo operations to complete
  }

  /**
   * Load template by code
   */
  loadTemplate(code: string): void {
    if (!code) return;

    this.isLoading = true;
    this.selectedReportCode = code;

    // Get schoolId from localStorage if available
    const schoolIdStr = localStorage.getItem('schoolId');
    const schoolId = schoolIdStr ? parseInt(schoolIdStr, 10) : undefined;

    this.reportTemplateService.getTemplateByCode(code, schoolId).subscribe({
      next: (template: ReportTemplateGetDTO) => {
        // Set the form value with the loaded template HTML
        const htmlContent = template.templateHtml || '';
        
        // Store HTML content for when editor is ready
        this.currentReportHtml = htmlContent;
        this.currentTemplateId = template.id;
        
        // Set HTML content - use our custom method if Quill is ready
        if (this.quillInstance) {
          // Quill is ready, use setEditorContent to properly convert HTML to Delta
          // This preserves spaces correctly
          this.setEditorContent(htmlContent);
          this.isLoading = false;
        } else {
          // Set via form control and wait for editor to initialize
          this.formGroup.get('text')?.setValue(htmlContent);
          // Editor will load content when it initializes (see onEditorInit)
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Error loading template:', error);
        // If template doesn't exist, start with empty editor
        this.formGroup.patchValue({ text: '' });
        this.currentTemplateId = undefined;
        this.isLoading = false;
        
        // Show info message (not error, as template might not exist yet)
        this.toastr.info('Template will be created when you save.', 'Template not found');
      }
    });
  }

  /**
   * Handle report type selection change
   */
  onReportTypeChange(): void {
    if (this.selectedReportOption) {
      this.loadTemplate(this.selectedReportOption.code);
    }
  }

  /**
   * Save template
   */
  saveTemplate(): void {
    if (!this.selectedReportCode) {
      this.toastr.warning('Please select a report type first.', 'No template selected');
      return;
    }

    // Get HTML content from Quill editor if available, otherwise from form control
    let templateHtml = '';
    if (this.quillInstance) {
      // Get HTML directly from Quill's root element to preserve table structure
      const root = this.quillInstance.root;
      templateHtml = root.innerHTML || this.formGroup.get('text')?.value || '';
    } else {
      templateHtml = this.formGroup.get('text')?.value || '';
    }
    
    if (!templateHtml.trim()) {
      this.toastr.warning('Template content cannot be empty.', 'Empty template');
      return;
    }

    this.isSaving = true;

    // Get schoolId from localStorage if available
    const schoolIdStr = localStorage.getItem('schoolId');
    const schoolId = schoolIdStr ? parseInt(schoolIdStr, 10) : undefined;

    const saveDto = {
      name: this.reportOptions.find(opt => opt.code === this.selectedReportCode)?.label || 'Report Template',
      code: this.selectedReportCode,
      schoolId: schoolId,
      templateHtml: templateHtml
    };

    this.reportTemplateService.saveTemplate(saveDto, schoolId).subscribe({
      next: (template: ReportTemplateGetDTO) => {
        this.currentTemplateId = template.id;
        this.currentReportHtml = template.templateHtml;
        this.isSaving = false;
        
        this.toastr.success('Template saved successfully.', 'Success');
      },
      error: (error) => {
        console.error('Error saving template:', error);
        this.isSaving = false;
        
        this.toastr.error(error.message || 'Failed to save template.', 'Error');
      }
    });
  }

}
