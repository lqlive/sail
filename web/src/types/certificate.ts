export interface SNI {
  id: string;
  name: string;
  hostName: string;
  createdAt: string;
  updatedAt: string;
}

export interface Certificate {
  id: string;
  name?: string;
  cert?: string;
  key?: string;
  snIs?: SNI[];
  createdAt: string;
  updatedAt: string;
}

