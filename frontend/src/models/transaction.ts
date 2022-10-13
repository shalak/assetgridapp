import Decimal from "decimal.js";
import { DateTime } from "luxon";
import { Account } from "./account";
import { SearchResponse } from "./search";

export interface Transaction {
    id: number
    source: Account | null
    destination: Account | null
    dateTime: DateTime
    identifiers: string[]
    lines: TransactionLine[]
    isSplit: boolean
    description: string

    total: Decimal
}

export interface CreateTransaction {
    sourceId: number | null
    destinationId: number | null
    dateTime: DateTime
    description: string
    identifiers: string[]
    total: Decimal
    lines: TransactionLine[]
    isSplit: boolean
}

export interface UpdateTransaction {
    identifiers?: string[]
    sourceId?: number | null
    destinationId?: number | null
    dateTime?: DateTime
    description?: string
    total?: Decimal
    lines?: TransactionLine[]
    isSplit?: boolean
}

export interface TransactionLine {
    amount: Decimal
    description: string
    category: string
}

export type TransactionListResponse = {
    total: Decimal
} & SearchResponse<Transaction>;
