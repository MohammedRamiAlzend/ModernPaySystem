export enum DepartmentType {
    Country = 1,        // State (Syria)
    Governorate = 2,    // Governorate (Rif Dimashq)
    District = 3,       // District (Ghouta Eastern)
    Municipality = 4,   // Municipality (Douma)
    Office = 5,         // Office (Technical Office Diwan)
    Unit = 6            // Other administrative unit
}

export interface Department {
    id: string;
    name: string;
    code?: string;
    description?: string;
    parentDepartmentId?: string;
    parentDepartmentName?: string;
    level: number;
    materializedPath?: string;
    type: DepartmentType;
    childrenCount: number;
    usersCount: number;
    createdAt?: string;
}

export interface DepartmentTree {
    id: string;
    name: string;
    code?: string;
    level: number;
    type: DepartmentType;
    children: DepartmentTree[];
}

export interface CreateDepartmentDto {
    name: string;
    code?: string;
    description?: string;
    parentDepartmentId?: string;
    type: DepartmentType;
}

export interface UpdateDepartmentDto {
    name?: string;
    code?: string;
    description?: string;
    parentDepartmentId?: string;
    type?: DepartmentType;
}
