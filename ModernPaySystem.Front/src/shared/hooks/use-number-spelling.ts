import { useQuery } from '@tanstack/react-query';
import { utilsService } from '../api/services/utils-service';
import { QUERY_STRATEGIES, UpdateStrategy } from '../constants/query-strategies';

/**
 * Hook to convert a number to Arabic spelling (Tafqeet).
 * Uses BACKGROUND strategy with a long stale time since number spellings don't change.
 * @param number The number to convert
 */
export const useNumberSpelling = (number: number | string | undefined | null) => {
    // Ensure we have a valid number
    const numericValue = typeof number === 'string' ? parseFloat(number) : number;
    const isValid = numericValue !== undefined && numericValue !== null && !isNaN(numericValue);

    return useQuery({
        queryKey: ['number-spelling', numericValue],
        queryFn: () => utilsService.numberSearchSpelling(numericValue as number),
        enabled: isValid,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND],
        staleTime: Infinity, // The spelling of a number never changes
    });
};
