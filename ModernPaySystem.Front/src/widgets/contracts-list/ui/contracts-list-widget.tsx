import { Search, Plus, Edit, Trash2, Printer, Filter } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { prefetchRoute } from '@/app/router';
import { useContractManager } from '@/features/contracts/model/contract-manager';
import { SearchModal } from '@/shared/ui/common/search-modal';
import { Button } from '@/shared/ui/button';
import { useUIStore } from '@/app/store/uiStore';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shared/ui/table"
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/shared/ui/tooltip"
import { useState } from 'react';

export const ContractsListWidget = () => {
    const { showConfirm } = useUIStore();
    const navigate = useNavigate();
    const { contracts, setSearchTerm } = useContractManager();
    // const { contracts, setSearchTerm, addContract, updateContract, getContract } = useContractManager();
    const [searchModalOpen, setSearchModalOpen] = useState(false);

    const handleEdit = (id: number) => {
        navigate(`/contracts/edit/${id}`);
    };

    const confirmDelete = (id: number) => {
        showConfirm({
            title: 'حذف العقد',
            message: 'هل أنت متأكد من حذف العقد؟ هذا الإجراء نهائي ولا يمكن التراجع عنه. سيتم حذف كافة البيانات المرتبطة بهذا العقد من النظام بشكل دائم.',
            variant: 'destructive',
            confirmLabel: 'حذف العقد',
            onConfirm: () => {
                // In a real implementation, we would call the delete function here
                console.log('Contract deleted:', id);
            }
        });
    };

    return (
        <AnimatedContainer className="space-y-6">
            <TooltipProvider>
                <div className="bg-card rounded-2xl border shadow-sm p-6 overflow-hidden">
                    <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-8 px-2">
                        <div className="space-y-1">
                            <h2 className="text-3xl font-bold tracking-tight text-right">عقود الإيجار</h2>
                            <p className="text-muted-foreground text-sm text-right">إدارة ومتابعة كافة عقود الإيجار المسجلة في النظام.</p>
                        </div>

                        <div className="flex flex-wrap items-center gap-3 w-full md:w-auto">
                            <div className="relative flex-1 md:w-80 group">
                                <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground group-focus-within:text-primary transition-colors" />
                                <input
                                    type="text"
                                    placeholder="بحث برقم العقد، الحي، أو الوصف..."
                                    className="w-full h-11 pr-10 pl-4 rounded-xl border border-border bg-background text-sm focus:ring-2 focus:ring-primary/10 focus:border-primary outline-none transition-all"
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                />
                            </div>

                            <Button variant="outline" onClick={() => setSearchModalOpen(true)} className="gap-2 h-11 rounded-xl">
                                <Filter className="w-4 h-4" /> بحث متقدم
                            </Button>

                            <Button
                                onClick={() => navigate('/contracts/new')}
                                onMouseEnter={() => prefetchRoute('/contracts/new')}
                                className="gap-2 h-11 rounded-xl shadow-lg shadow-primary/20"
                            >
                                <Plus className="w-4 h-4" /> إضافة عقد جديد
                            </Button>
                        </div>
                    </div>

                    <div className="rounded-xl border border-border/60 overflow-hidden">
                        <Table>
                            <TableHeader className="bg-muted/30">
                                <TableRow>
                                    <TableHead className="text-center w-16">#</TableHead>
                                    <TableHead>رقم العقد</TableHead>
                                    <TableHead>تاريخ العقد</TableHead>
                                    <TableHead>بدل الإيجار</TableHead>
                                    <TableHead>الحي / المنطقة</TableHead>
                                    <TableHead>المدينة</TableHead>
                                    <TableHead className="text-left px-6">الإجراءات</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {contracts.length > 0 ? (
                                    contracts.map((contract, index) => (
                                        <TableRow key={contract.id} className="group transition-colors">
                                            <TableCell className="text-center text-muted-foreground font-mono">{index + 1}</TableCell>
                                            <TableCell className="font-semibold text-primary">{contract.contractNo}</TableCell>
                                            <TableCell>{contract.contractDate}</TableCell>
                                            <TableCell className="font-mono font-medium">{contract.rentAmount.toLocaleString()} </TableCell>
                                            <TableCell>{contract.district}</TableCell>
                                            <TableCell>
                                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary/10 text-primary">
                                                    {contract.city}
                                                </span>
                                            </TableCell>
                                            <TableCell className="text-left px-6">
                                                <div className="flex justify-end gap-1">
                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
                                                            <Button
                                                                variant="ghost"
                                                                size="icon"
                                                                onClick={() => handleEdit(contract.id)}
                                                                onMouseEnter={() => prefetchRoute(`/contracts/edit/${contract.id}`)}
                                                                className="w-8 h-8 rounded-lg text-primary hover:bg-primary/10"
                                                            >
                                                                <Edit className="w-4 h-4" />
                                                            </Button>
                                                        </TooltipTrigger>
                                                        <TooltipContent>تعديل العقد</TooltipContent>
                                                    </Tooltip>

                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
                                                            <Button
                                                                variant="ghost"
                                                                size="icon"
                                                                onClick={() => confirmDelete(contract.id)}
                                                                className="w-8 h-8 rounded-lg text-destructive hover:bg-destructive/10"
                                                            >
                                                                <Trash2 className="w-4 h-4" />
                                                            </Button>
                                                        </TooltipTrigger>
                                                        <TooltipContent>حذف العقد</TooltipContent>
                                                    </Tooltip>

                                                    <Tooltip>
                                                        <TooltipTrigger asChild>
                                                            <Button
                                                                variant="ghost"
                                                                size="icon"
                                                                className="w-8 h-8 rounded-lg text-muted-foreground hover:bg-muted"
                                                            >
                                                                <Printer className="w-4 h-4" />
                                                            </Button>
                                                        </TooltipTrigger>
                                                        <TooltipContent>طباعة العقد</TooltipContent>
                                                    </Tooltip>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))
                                ) : (
                                    <TableRow>
                                        <TableCell colSpan={7} className="h-40 text-center">
                                            <div className="flex flex-col items-center gap-3 text-muted-foreground">
                                                <div className="p-4 bg-muted/50 rounded-full">
                                                    <Search className="w-8 h-8" />
                                                </div>
                                                <p className="text-base font-medium">لا توجد عقود تطابق بحثك حالياً</p>
                                                <Button variant="link" onClick={() => setSearchTerm('')} className="text-primary">إعادة ضبط البحث</Button>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                )}
                            </TableBody>
                        </Table>
                    </div>
                </div>
            </TooltipProvider>

            {/* Modals & Dialogs */}
            <SearchModal
                open={searchModalOpen}
                onClose={() => setSearchModalOpen(false)}
            />
        </AnimatedContainer>
    );
};