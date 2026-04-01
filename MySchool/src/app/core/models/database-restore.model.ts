export interface RestoredFileMappingDTO {
  logicalName: string;
  fileType: string;
  newPhysicalPath: string;
}

export interface DatabaseRestoreResultDTO {
  databaseName: string;
  backupPathOnServer: string;
  fileMappings: RestoredFileMappingDTO[];
}

export interface RestoreHistoryRow {
  at: Date;
  success: boolean;
  fileName: string;
  databaseName?: string;
  errorMessages?: string[];
  result?: DatabaseRestoreResultDTO;
}
