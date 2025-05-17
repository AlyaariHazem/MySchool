import { createFeatureSelector } from '@ngrx/store';

export const selectLanguage = createFeatureSelector<string>('language');