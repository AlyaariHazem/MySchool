import {
  Component,
  ViewChild,
  ViewContainerRef,
  Type,
  ElementRef,
  OnInit,
  inject,
  AfterViewInit,
  OnDestroy,
  HostListener,
  ChangeDetectorRef,
} from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Editor } from 'primeng/editor';
import Quill from 'quill';

import { StudentMonthResultComponent } from '../../report/student-month-result/student-month-result.component';
import { AccountReportComponent } from '../../report/account-report/account-report.component';
import {
  ReportTemplateService,
  ReportTemplateGetDTO,
  ReportTemplatePlaceholderDto,
} from '../../core/services/report-template.service';

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
export class AllotmentComponent implements OnInit, AfterViewInit, OnDestroy {

  /* dynamic outlet */
  @ViewChild('reportContainer', { read: ViewContainerRef, static: true })
  reportContainer!: ViewContainerRef;

  /* element we convert to PDF */
  @ViewChild('editorPreview', { static: false })
  editorPreview!: ElementRef<HTMLDivElement>;

  /** PrimeNG `p-editor` — use `getQuill()` / DOM for persisted HTML, not form controls alone. */
  @ViewChild('editor') editorCmp!: Editor;

  private readonly fb = inject(FormBuilder);

  /**
   * `text` (and optional `title`) exist only to keep `p-editor` wired to Angular forms.
   * They are NOT the source of truth for persisted HTML because PrimeNG/Quill may normalize
   * values through `getSemanticHTML()` — keep the `getSemanticHTML` patch for internal CVA sync,
   * but never use `formGroup.controls.text.value` as what you save.
   */
  formGroup = this.fb.group({
    title: [''],
    text: [''],
  });

  /** Last reliable snapshot of `quill.root.innerHTML` (editor DOM), updated on change / insert. */
  currentReportHtml = '';

  /** Cached Quill instance from `onInit` (same as `editorCmp.getQuill()` once ready). */
  private quillInstance: Quill | null = null;

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
  private cdr = inject(ChangeDetectorRef);

  // Template state
  currentTemplateId?: number;
  isLoading = false;
  isSaving = false;

  /** Merge fields for # autocomplete (from API, scoped by report template code). */
  placeholderColumns: ReportTemplatePlaceholderDto[] = [];
  filteredPlaceholderColumns: ReportTemplatePlaceholderDto[] = [];
  mentionOpen = false;
  mentionStartIndex = 0;
  mentionTop = 0;
  mentionLeft = 0;
  mentionHighlightIndex = 0;
  private lastMentionFilter = '';
  private suppressMentionSync = false;
  private quillScrollEl: HTMLElement | null = null;
  private boundOnQuillScroll = () => this.positionMentionDropdown();

  private readonly onQuillTextChangeForMention = (_d: unknown, _o: unknown, source: string) => {
    if (source !== 'user' || this.suppressMentionSync) {
      return;
    }
    this.syncMentionFromEditor();
  };

  private readonly onQuillSelectionChangeForMention = () => {
    if (this.suppressMentionSync) {
      return;
    }
    this.syncMentionFromEditor();
  };

  ngOnInit(): void {
    // Set initial selected option
    this.selectedReportOption = this.reportOptions.find(opt => opt.code === this.selectedReportCode) || this.reportOptions[0];
    // Load default template on init
    this.loadTemplate(this.selectedReportCode);
  }

  ngAfterViewInit(): void {
    // Quill instance will be set via onEditorInit
  }

  ngOnDestroy(): void {
    if (this.quillInstance) {
      this.quillInstance.off('text-change', this.onQuillTextChangeForMention);
      this.quillInstance.off('selection-change', this.onQuillSelectionChangeForMention);
    }
    if (this.quillScrollEl) {
      this.quillScrollEl.removeEventListener('scroll', this.boundOnQuillScroll);
      this.quillScrollEl = null;
    }
  }

  @HostListener('document:keydown', ['$event'])
  onDocumentKeydown(event: KeyboardEvent): void {
    if (!this.mentionOpen || !this.filteredPlaceholderColumns.length) {
      return;
    }
    const t = event.target as Node | null;
    if (t && !this.editorPreview?.nativeElement.contains(t)) {
      return;
    }
    if (event.key === 'Escape') {
      event.preventDefault();
      this.closeMention();
      return;
    }
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.mentionHighlightIndex = Math.min(
        this.mentionHighlightIndex + 1,
        this.filteredPlaceholderColumns.length - 1
      );
      return;
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.mentionHighlightIndex = Math.max(this.mentionHighlightIndex - 1, 0);
      return;
    }
    if (event.key === 'Enter' || event.key === 'Tab') {
      event.preventDefault();
      const pick = this.filteredPlaceholderColumns[this.mentionHighlightIndex];
      if (pick) {
        this.selectPlaceholder(pick);
      }
    }
  }

  /**
   * PrimeNG Editor (Quill 2) uses `getSemanticHTML()` for ngModel updates — that API strips
   * inline `style`, collapses structure, and drops lines like the centered title. Preserve
   * the real DOM string for this template editor.
   */
  private patchQuillPreserveTemplateHtml(quill: Quill): void {
    const q = quill as Quill & { getSemanticHTML?: () => string };
    q.getSemanticHTML = () => q.root.innerHTML;
  }

  /**
   * Visual / persisted HTML as rendered inside `.ql-editor`.
   * Read from the DOM — not from `formControl.value` — because Quill semantic HTML can strip
   * inline styles and simplify structure.
   *
   * Trade-off: DOM HTML preserves formatting; Delta / semantic HTML is structurally cleaner for round-trips.
   */
  private getEditorDomHtml(): string {
    const quill = (this.editorCmp?.getQuill() ?? this.quillInstance) as Quill | null | undefined;
    if (!quill?.root) {
      return '';
    }
    return quill.root.innerHTML ?? '';
  }

  private isEditorDomEmpty(html: string | null | undefined): boolean {
    if (!html) {
      return true;
    }

    const normalized = html
      .replace(/&nbsp;/g, ' ')
      .replace(/\u00A0/g, ' ')
      .trim();

    if (!normalized) {
      return true;
    }

    const emptyPatterns = new Set([
      '<p><br></p>',
      '<div><br></div>',
      '<p></p>',
      '<div></div>',
    ]);

    if (emptyPatterns.has(normalized)) {
      return true;
    }

    const textOnly = normalized
      .replace(/<br\s*\/?>/gi, '')
      .replace(/<\/p>|<\/div>/gi, '')
      .replace(/<p>|<div>/gi, '')
      .replace(/<[^>]*>/g, '')
      .trim();

    return textOnly.length === 0;
  }

  /**
   * Handle editor initialization - get Quill instance
   */
  onEditorInit(event: any): void {
    if (event && event.editor) {
      const editor = (this.editorCmp?.getQuill() ?? event.editor) as Quill;
      this.quillInstance = editor;
      this.patchQuillPreserveTemplateHtml(editor);
      this.setupQuillPasteHandler();
      this.setupPlaceholderMention();

      // Configure Quill to preserve spaces
      this.configureQuillForSpaces();

      // PrimeNG already ran setContents(clipboard.convert(...)) which stripped CSS — re-apply full HTML
      if (this.currentReportHtml?.trim()) {
        setTimeout(() => this.setEditorContent(this.currentReportHtml), 0);
      }
    }
  }

  /**
   * Load merge-field names for the selected report template (shown after typing #).
   */
  private loadPlaceholders(code: string): void {
    if (!code) {
      this.placeholderColumns = [];
      return;
    }
    this.reportTemplateService.getTemplatePlaceholders(code).subscribe({
      next: (rows) => {
        this.placeholderColumns = rows.filter((r) => r.name?.trim());
        this.cdr.markForCheck();
      },
    });
  }

  /**
   * #… mention: detect unfinished #field, show dropdown, insert #Name# on pick.
   */
  private setupPlaceholderMention(): void {
    if (!this.quillInstance) {
      return;
    }

    const ql = this.quillInstance as Quill & { __mentionBound?: boolean };
    if (ql.__mentionBound) {
      return;
    }
    ql.__mentionBound = true;

    const container = ql.root.closest('.ql-container') as HTMLElement | null;
    if (container && container !== this.quillScrollEl) {
      if (this.quillScrollEl) {
        this.quillScrollEl.removeEventListener('scroll', this.boundOnQuillScroll);
      }
      this.quillScrollEl = container;
      this.quillScrollEl.addEventListener('scroll', this.boundOnQuillScroll, { passive: true });
    }

    ql.on('text-change', this.onQuillTextChangeForMention);
    ql.on('selection-change', this.onQuillSelectionChangeForMention);
  }

  private syncMentionFromEditor(): void {
    if (!this.quillInstance || this.suppressMentionSync) {
      return;
    }
    const sel = this.quillInstance.getSelection();
    if (!sel) {
      this.closeMention();
      return;
    }
    const cursor = sel.index;
    const text = this.quillInstance.getText(0, cursor);
    const lastHash = text.lastIndexOf('#');
    if (lastHash === -1) {
      this.closeMention();
      return;
    }
    const afterHash = text.slice(lastHash + 1);
    if (afterHash.includes('#')) {
      this.closeMention();
      return;
    }
    if (!/^[\w]*$/.test(afterHash)) {
      this.closeMention();
      return;
    }

    const filterLower = afterHash.toLowerCase();
    const filtered = this.placeholderColumns.filter((p) =>
      p.name.toLowerCase().includes(filterLower)
    );
    if (!filtered.length) {
      this.closeMention();
      return;
    }

    if (afterHash !== this.lastMentionFilter) {
      this.mentionHighlightIndex = 0;
      this.lastMentionFilter = afterHash;
    }

    this.mentionOpen = true;
    this.mentionStartIndex = lastHash;
    this.filteredPlaceholderColumns = filtered;
    this.positionMentionDropdown();
    this.cdr.markForCheck();
  }

  private positionMentionDropdown(): void {
    if (!this.quillInstance || !this.editorPreview) {
      return;
    }
    try {
      const bounds = this.quillInstance.getBounds(this.mentionStartIndex);
      if (!bounds) {
        return;
      }
      const editor = this.quillInstance.root;
      const editorRect = editor.getBoundingClientRect();
      const preview = this.editorPreview.nativeElement;
      const previewRect = preview.getBoundingClientRect();
      this.mentionTop = editorRect.top - previewRect.top + bounds.top + bounds.height + preview.scrollTop;
      this.mentionLeft = editorRect.left - previewRect.left + bounds.left + preview.scrollLeft;
    } catch {
      this.mentionTop = 0;
      this.mentionLeft = 0;
    }
  }

  closeMention(): void {
    this.mentionOpen = false;
    this.filteredPlaceholderColumns = [];
    this.lastMentionFilter = '';
    this.cdr.markForCheck();
  }

  selectPlaceholder(item: ReportTemplatePlaceholderDto): void {
    if (!this.quillInstance || !item.name) {
      this.closeMention();
      return;
    }
    const sel = this.quillInstance.getSelection(true);
    const end = sel ? sel.index : this.mentionStartIndex;
    const len = Math.max(0, end - this.mentionStartIndex);
    const token = `#${item.name}#`;

    this.suppressMentionSync = true;
    this.quillInstance.deleteText(this.mentionStartIndex, len, 'user');
    this.quillInstance.insertText(this.mentionStartIndex, token, 'user');
    this.quillInstance.setSelection(this.mentionStartIndex + token.length, 0, 'user');
    this.suppressMentionSync = false;

    this.currentReportHtml = this.getEditorDomHtml();
    this.formGroup.patchValue({ text: this.currentReportHtml }, { emitEvent: false });

    this.closeMention();
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
   * Quill's clipboard converts HTML → Delta and drops most inline CSS (font-size, color, text-align on divs, etc.).
   * When the template relies on inline styles or flexbox, set .ql-editor.innerHTML directly (same as flex path).
   */
  private htmlRequiresRawQuillRoot(html: string): boolean {
    if (!html) {
      return false;
    }
    const lower = html.toLowerCase();
    if (lower.includes('display:flex') || lower.includes('display: flex')) {
      return true;
    }
    // Any inline style= is stripped or flattened by dangerouslyPasteHTML — keep raw HTML
    return /\sstyle\s*=/.test(html);
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
          const useRawHtml = this.htmlRequiresRawQuillRoot(html);

          if (useRawHtml) {
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

            this.currentReportHtml = newHtml;
            this.formGroup.patchValue({ text: this.currentReportHtml }, { emitEvent: false });
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
            
            this.currentReportHtml = updatedHtml;
            this.formGroup.patchValue({ text: this.currentReportHtml }, { emitEvent: false });
            
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
   * Load/replace template HTML. `patchValue` keeps the form bound to `p-editor` only — do not treat
   * `formControl.value` as the persisted/visual source afterward.
   */
  private setEditorContent(html: string): void {
    this.currentReportHtml = html ?? '';

    this.formGroup.patchValue({ text: this.currentReportHtml }, { emitEvent: false });

    const quill = (this.editorCmp?.getQuill() ?? this.quillInstance) as Quill | null | undefined;
    if (!quill) {
      return;
    }

    if (!this.currentReportHtml.trim()) {
      try {
        const Delta = (Quill as any).import('delta');
        quill.setContents(new Delta(), 'silent');
      } catch (e) {
        console.warn('Error clearing editor:', e);
      }
      return;
    }

    try {
      if (quill.root.innerHTML !== this.currentReportHtml) {
        quill.root.innerHTML = this.currentReportHtml;
        quill.update('silent');
      }
    } catch (e) {
      console.warn('Error setting editor content:', e);
    }
  }


  /** Sync snapshot from editor DOM only — never use `event.htmlValue` as the source of truth. */
  onEditorTextChange(_event: unknown): void {
    this.currentReportHtml = this.getEditorDomHtml();
  }

  /**
   * Load template by code
   */
  loadTemplate(code: string): void {
    if (!code) return;

    this.isLoading = true;
    this.selectedReportCode = code;
    this.loadPlaceholders(code);

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
          this.formGroup.patchValue({ text: htmlContent }, { emitEvent: false });
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Error loading template:', error);
        // If template doesn't exist, start with empty editor
        this.formGroup.patchValue({ title: '', text: '' });
        this.currentReportHtml = '';
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

    let templateHtml = this.getEditorDomHtml();

    if (this.isEditorDomEmpty(templateHtml) && !this.isEditorDomEmpty(this.currentReportHtml)) {
      templateHtml = this.currentReportHtml;
    }

    if (this.isEditorDomEmpty(templateHtml)) {
      this.toastr.warning('محتوى التقرير فارغ.', 'تنبيه');
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
