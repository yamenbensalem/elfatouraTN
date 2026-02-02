// Core models for Tunisian E-Invoice Application

export interface Client {
  id: string;
  matriculeFiscal: string;
  name: string;
  legalForm?: string;
  registrationNumber?: string;
  capital?: number;
  addressDescription?: string;
  street?: string;
  city?: string;
  postalCode?: string;
  countryCode: string;
  phone?: string;
  fax?: string;
  email?: string;
  website?: string;
  bankAccountNumber?: string;
  bankCode?: string;
  bankName?: string;
  ttnAccountMode?: string;
  ttnAccountRank?: string;
  ttnProfile?: string;
  ttnClientCode?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface LineItem {
  itemIdentifier: string;
  itemCode: string;
  itemDescription: string;
  language: string;
  quantity: number;
  measurementUnit: string;
  unitPriceExcludingTax: number;
  totalExcludingTax: number;
  taxTypeCode: string;
  taxTypeName: string;
  taxRate: number;
  taxAmount: number;
}

export interface InvoiceRequest {
  documentIdentifier: string;
  documentType: string;
  invoiceDate: Date;
  dueDate?: Date;
  periodFrom?: string;
  periodTo?: string;
  sender: PartnerInfo;
  receiver: PartnerInfo;
  lineItems: LineItem[];
  paymentSections?: PaymentSection[];
  stampDuty: number;
  freeText?: string;
  specialConditions?: string[];
}

export interface PartnerInfo {
  identifierType: string;
  identifier: string;
  name: string;
  address: AddressInfo;
  contacts?: ContactInfo[];
  accountMode?: string;
  accountRank?: string;
  profile?: string;
  clientCode?: string;
  registrationNumber?: string;
  legalForm?: string;
  capital?: number;
  bankAccounts?: BankAccountInfo[];
}

export interface AddressInfo {
  description: string;
  street: string;
  city: string;
  postalCode: string;
  country: string;
  language: string;
}

export interface ContactInfo {
  type: string;
  identifier?: string;
  name?: string;
  value: string;
}

export interface BankAccountInfo {
  accountNumber: string;
  ownerIdentifier: string;
  bankCode: string;
  branchIdentifier: string;
  institutionName: string;
  functionCode: string;
}

export interface PaymentSection {
  paymentTermsTypeCode: string;
  paymentTermsDescription: string;
  bankAccount: BankAccountInfo;
}

export interface InvoiceResponse {
  success: boolean;
  message: string;
  xmlWithoutSignature?: string;
  xmlWithSignature?: string;
  pdfDocument?: string;
  ttnReference?: string;
  qrCode?: string;
  validationErrors?: ValidationError[];
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface InvoiceRecord {
  id: string;
  invoiceNumber: number;
  documentIdentifier: string;
  documentTypeCode: string;
  documentTypeName: string;
  invoiceDate: Date;
  dueDate?: Date;
  senderId: string;
  senderName?: string;
  receiverId: string;
  receiverName?: string;
  totalExcludingTax: number;
  totalTaxAmount: number;
  stampDuty: number;
  totalIncludingTax: number;
  status: InvoiceStatus;
  statusMessage?: string;
  ttnReference?: string;
  createdAt: Date;
  validatedAt?: Date;
}

export enum InvoiceStatus {
  Draft = 0,
  Generated = 1,
  Signed = 2,
  Sent = 3,
  Pending = 4,
  Validated = 5,
  Rejected = 6,
  Cancelled = 7
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
