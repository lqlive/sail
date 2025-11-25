export interface SNI {
  id: string;
  name: string;
  hostName: string;
  createdAt: string;
  updatedAt: string;
}

export interface Certificate {
  id: string;
  cert?: string;
  key?: string;
  snis?: SNI[];
  createdAt: string;
  updatedAt: string;
}

