// import * as React from 'react';
import { FC, useEffect, useState } from 'react';
import { Card } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { getResponsesByFormId, deleteResponse, type FormResponse } from '@/shared/lib/form-engine/responses';
import { Button } from '@/shared/ui/button';
import { Trash2, Calendar, FileJson, Copy, Check, Search, Download, Eye } from 'lucide-react';
import type { FormSchema } from '@/entities/form/model/types';
import { ResponseDetailsModal } from './response-details-modal';

interface ResponsesListProps {
    schema: FormSchema;
}

export const ResponsesList: FC<ResponsesListProps> = ({ schema }) => {
    const [responses, setResponses] = useState<FormResponse[]>([]);
    const [copiedId, setCopiedId] = useState<string | null>(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [viewResponse, setViewResponse] = useState<FormResponse | null>(null);

    useEffect(() => {
        setResponses(getResponsesByFormId(schema.id));
    }, [schema.id]);

    const handleDelete = (id: string) => {
        if (confirm('هل أنت متأكد من حذف هذا الرد؟')) {
            deleteResponse(id);
            setResponses(prev => prev.filter(r => r.id !== id));
        }
    };

    const handleCopy = (id: string, data: any) => {
        navigator.clipboard.writeText(JSON.stringify(data, null, 2));
        setCopiedId(id);
        setTimeout(() => setCopiedId(null), 2000);
    };

    const exportToJson = () => {
        const dataStr = "data:text/json;charset=utf-8," + encodeURIComponent(JSON.stringify(responses, null, 2));
        const downloadAnchorNode = document.createElement('a');
        downloadAnchorNode.setAttribute("href", dataStr);
        downloadAnchorNode.setAttribute("download", `responses_${schema.id}.json`);
        document.body.appendChild(downloadAnchorNode);
        downloadAnchorNode.click();
        downloadAnchorNode.remove();
    };

    const filteredResponses = responses.filter(r =>
        JSON.stringify(r.data).toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (responses.length === 0) {
        return (
            <Card className="p-12 text-center animate-in fade-in zoom-in duration-300 border-dashed ">
                <div className="flex flex-col items-center gap-4">
                    <div className="w-20 h-20 rounded-full bg-primary/10 flex items-center justify-center animate-pulse">
                        <FileJson className="w-10 h-10 text-primary" />
                    </div>
                    <div>
                        <h3 className="text-2xl font-bold bg-clip-text text-transparent">لا يوجد ردود حالياً</h3>
                        <p className=" mt-2 max-w-sm mx-auto">عندما يقوم المستخدمون بملء هذا النموذج وحفظه، ستظهر ردودهم هنا بشكل تلقائي.</p>
                    </div>
                </div>
            </Card>
        );
    }

    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div className="relative w-full md:w-96 group">
                    <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4  group-focus-within:text-primary transition-colors" />
                    <input
                        type="text"
                        placeholder="بحث في الردود..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="w-full pr-10 pl-4 py-2 bg-transparent rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all shadow-sm"
                    />
                </div>
                <div className="flex gap-2 w-full md:w-auto">
                    <Button onClick={exportToJson} variant="outline" className="gap-2 rounded-xl border-primary/20 hover:bg-primary/5 text-primary w-full md:w-auto">
                        <Download className="w-4 h-4" /> تصدير JSON
                    </Button>
                </div>
            </div>

            <Card className="overflow-hidden border-0 shadow-2xl shadow-primary/5 rounded-2xl">
                <div className="overflow-x-auto">
                    <Table>
                        <TableHeader className="backdrop-blur-sm">
                            <TableRow>
                                <TableHead className="font-bold py-4">التاريخ والوقت</TableHead>
                                <TableHead className="font-bold py-4">البيانات المعبأة</TableHead>
                                <TableHead className="text-left font-bold py-4">إجراءات</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {filteredResponses.length > 0 ? filteredResponses.map((response) => (
                                <TableRow key={response.id} className="transition-colors hover:bg-primary/5 group">
                                    <TableCell className="whitespace-nowrap">
                                        <div className="flex flex-col">
                                            <div className="flex items-center gap-2 font-medium">
                                                <Calendar className="w-4 h-4 text-primary" />
                                                {new Date(response.submittedAt).toLocaleDateString('ar-EG')}
                                            </div>
                                            <span className="text-xs  mr-6">
                                                {new Date(response.submittedAt).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}
                                            </span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="min-w-[300px]">
                                        <div className="relative group/json">
                                            <pre className="text-xs transition-colors p-4 rounded-xl max-h-[150px] overflow-auto border font-mono leading-relaxed shadow-inner">
                                                {JSON.stringify(response, null, 2)}
                                            </pre>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                onClick={() => handleCopy(response.id, response)}
                                                className="absolute top-2 left-2 opacity-0 group-hover/json:opacity-100 transition-opacity bg-white/80 backdrop-blur shadow-sm h-8 w-8"
                                            >
                                                {copiedId === response.id ? <Check className="w-4 h-4 " /> : <Copy className="w-4 h-4" />}
                                            </Button>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-left">
                                        <div className="flex gap-1 justify-end">
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                onClick={() => setViewResponse(response)}
                                                className="text-primary hover:bg-primary/5 rounded-full"
                                            >
                                                <Eye className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                onClick={() => handleDelete(response.id)}
                                                className="text-red-400 hover:text-red-600 hover:bg-red-50 transition-colors rounded-full"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            )) : (
                                <TableRow>
                                    <TableCell colSpan={3} className="text-center py-10 text-gray-400">
                                        لا توجد نتائج تطابق البحث
                                    </TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </div>
            </Card>

            <ResponseDetailsModal
                isOpen={!!viewResponse}
                onClose={() => setViewResponse(null)}
                schema={schema}
                response={viewResponse}
            />
        </div>
    );
};
