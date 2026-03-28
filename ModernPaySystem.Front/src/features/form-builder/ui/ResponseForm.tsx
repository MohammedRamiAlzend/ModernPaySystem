import { Card } from '@/shared/ui/card';
import { Reply, Paperclip, FileText, X } from 'lucide-react';
import { Label } from '@/shared/ui/label';
import { Input } from '@/shared/ui/input';
import { Textarea } from '@/shared/ui/textarea';
import { Button } from '@/shared/ui/button';
import { User } from '@/app/store/authStore';

interface ResponseFormProps {
    requestId: string;
    comment: string;
    files: File[];
    isPending: boolean;
    currentUser: User | null;
    onCommentChange: (value: string) => void;
    onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onRemoveFile: (index: number) => void;
    onSubmit: () => void;
}

export const ResponseForm = ({
    requestId,
    comment,
    files,
    isPending,
    currentUser,
    onCommentChange,
    onFileChange,
    onRemoveFile,
    onSubmit
}: ResponseFormProps) => {
    return (
        <div className="space-y-6">
            <Card className="p-6 space-y-4 shadow-xl border-primary/10">
                <h2 className="text-xl font-semibold flex items-center gap-2 text-primary">
                    <Reply className="w-5 h-5" />
                    إرسال رد
                </h2>
                <div className="space-y-4">
                    {/* <div className="space-y-2">
                        <Label className="text-sm font-bold">معرف الطلب المستهدف</Label>
                        <Input
                            placeholder="اختر طلباً من القائمة أو أدخل المعرف"
                            value={requestId}
                            disabled
                            className="bg-muted/30 rounded-xl h-11"
                        />
                    </div> */}

                    <div className="space-y-2">
                        <Label className="text-sm font-bold">نص الرد / القرار</Label>
                        <Textarea
                            placeholder="اكتب تعليقك هنا..."
                            value={comment}
                            onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => onCommentChange(e.target.value)}
                            rows={5}
                            className="resize-none rounded-xl"
                        />
                    </div>

                    <div className="space-y-3">
                        <Label className="text-sm font-bold">المرفقات (صور، مستندات)</Label>
                        <div className="flex flex-col gap-3">
                            <Label
                                htmlFor="file-upload"
                                className="flex flex-col items-center justify-center w-full h-24 border-2 border-dashed border-muted-foreground/20 rounded-xl cursor-pointer hover:bg-muted/30 hover:border-primary/50 transition-all group"
                            >
                                <div className="flex flex-col items-center justify-center pt-5 pb-6">
                                    <Paperclip className="w-6 h-6 mb-2 text-muted-foreground group-hover:text-primary transition-colors" />
                                    <p className="text-xs text-muted-foreground">اضغط لرفع ملفات أو صور</p>
                                </div>
                                <input
                                    id="file-upload"
                                    type="file"
                                    className="hidden"
                                    multiple
                                    onChange={onFileChange}
                                />
                            </Label>

                            {files.length > 0 && (
                                <div className="space-y-2">
                                    {files.map((file, index) => (
                                        <div key={index} className="flex items-center justify-between p-2 bg-muted/50 rounded-lg text-xs border border-border">
                                            <div className="flex items-center gap-2 truncate">
                                                <FileText className="w-3 h-3 text-primary" />
                                                <span className="truncate max-w-[150px]">{file.name}</span>
                                            </div>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-6 w-6 rounded-full hover:bg-destructive/10 hover:text-destructive"
                                                onClick={() => onRemoveFile(index)}
                                            >
                                                <X className="w-3 h-3" />
                                            </Button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    <Button
                        onClick={onSubmit}
                        disabled={!requestId || isPending}
                        className="w-full h-12 text-md font-bold shadow-lg shadow-primary/20 rounded-xl"
                    >
                        {isPending ? 'جاري الإرسال...' : 'إرسال الرد النهائي'}
                    </Button>
                </div>
            </Card>

            <Card className="p-4 bg-muted/30 border-dashed text-[10px] text-muted-foreground rounded-xl">
                <p>• سيتم ربط هذا الرد بطلبك الملحق أعلاه.</p>
                <p>• تأكد من صحة البيانات قبل الضغط على إرسال.</p>
                <p>• المنفذ الحالي: {currentUser?.username}</p>
            </Card>
        </div>
    );
};
