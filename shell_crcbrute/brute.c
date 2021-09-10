#include <stdint.h>
#include <string.h>
#include <stdio.h>
#include <stdio.h>
#include <omp.h>

#define COUNT(x) (sizeof(x)/sizeof((x)[0]))


uint8_t packets_crc8[][13] = {
    { 0x03, 0x25, 0xED, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x23 },
    { 0x03, 0x25, 0xED, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x68 },
    { 0x03, 0x53, 0xD4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x71 },
    { 0x03, 0x59, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC1 },
    { 0x03, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42 },
    { 0x03, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x86 },
    { 0x03, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCD },
    { 0x03, 0x74, 0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6E },
    { 0x03, 0x9E, 0xF1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF1 },
    { 0x03, 0x9F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x39 },
    { 0x03, 0xBF, 0xA6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08 },
    { 0x03, 0xDC, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x8B },
    { 0x03, 0xE8, 0xC8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x67 },
    { 0x01, 0x9E, 0xF1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4B },
    { 0x83, 0x00, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBF }
};

uint8_t packets_crc16[][23] = {
    { 0x03, 0x17, 0xFE, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF1, 0xBE },
    { 0x03, 0xA1, 0xFC, 0xE7, 0x1F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5A, 0x03 },
    { 0x03, 0x3A, 0xFD, 0xE7, 0x1F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xD9, 0x22 },
    { 0x03, 0x39, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB6, 0x68 },
    { 0x03, 0x3A, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x01 },
    { 0x03, 0x3A, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x40 },
    { 0x03, 0x3A, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x01 },
    { 0x03, 0x3A, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x40 },
    { 0x03, 0xD3, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xD9, 0xED },
    { 0x03, 0xD3, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xD9, 0xED },
    { 0x03, 0xD3, 0xFD, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0xCD },
    { 0x03, 0xD3, 0xFD, 0xE7, 0x9F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA6, 0xA9 },
    { 0x03, 0xD3, 0xFD, 0xE7, 0x9F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0xE8 },
    { 0x03, 0x6A, 0xFE, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE9, 0x26 },
    { 0x03, 0x6A, 0xFE, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1A, 0x06 },
    { 0x03, 0x6A, 0xFE, 0xE7, 0x9F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x96, 0x62 },
    { 0x03, 0x6A, 0xFE, 0xE7, 0x9F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x42 },
    { 0x03, 0x6A, 0xFE, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1A, 0x06 },
    { 0x03, 0x6A, 0xFE, 0xE7, 0x5F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE9, 0x26 },
    { 0x03, 0x17, 0xFE, 0xE7, 0x9F, 0x80, 0xA1, 0x84, 0x12, 0x4A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9B, 0x9B },
    { 0x03, 0x17, 0xFE, 0xE7, 0x9F, 0x80, 0xA1, 0x84, 0xF2, 0x5F, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x1E }
};

uint8_t crc8(uint8_t *data, int start, int length, uint8_t init)
{
    uint32_t crc = init;

    for (uint32_t i = 0; i < length; i++)
    {
        uint32_t inData = data[start + i];
        for (uint32_t j = 0; j < 8; j++)
        {
            uint32_t mix = (crc ^ inData) & 0x80;
            crc <<= 1;
            if (mix != 0)
            {
                //crc ^= 0xd5;
                //crc ^= 0x9b;
                //crc ^= 0x1d;
                crc ^= 0x07;
                //crc ^= 0x9b;
                //crc ^= 0x9b;
            }
            inData <<= 1;
        }
    }
    return (uint8_t)crc;
}

uint16_t reflect(uint16_t inData, int width)
{
    uint16_t resByte = 0;

    for (uint8_t i = 0; i < width; i++)
    {
        if ((inData & (1 << i)) != 0)
        {
            resByte |= ( (1 << (width-1 - i)) & 0xFFFF);
        }
    }

    return resByte;
}

uint16_t crc16(uint8_t *data, int start, int length, uint16_t init)
{
    uint16_t table[256] =
    {
        0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
        0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
        0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
        0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
        0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
        0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
        0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
        0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
        0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
        0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
        0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
        0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
        0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
        0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
        0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
        0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
    };
    uint16_t crc = init;

    for (int i = 0; i < length; ++i)
    {
        crc = (crc ^ (reflect(data[start + i], 8)<< (16 - 8)));
        int pos = (crc >> (16 - 8)) & 0xFF;
        crc = (crc << 8);
        crc = (crc ^ table[pos]);
    }
    return reflect(crc, 16);
}

void brute_crc8()
{
    uint8_t buffer[32];
    int pkt_start = 0;
    int pkt_len = 12;

    memcpy(&buffer[pkt_start], packets_crc8[0], pkt_len + 1);

    for(uint64_t test_value = 0; test_value < 0x100; test_value++)
    {
        if(crc8(buffer, 0, pkt_start + pkt_len, test_value) == buffer[pkt_start + pkt_len])
        {
            uint8_t buffer_check[32];

            for(int pkt = 1; pkt < COUNT(packets_crc8); pkt++)
            {
                memcpy(&buffer_check[pkt_start], packets_crc8[pkt], pkt_len + 1);
                if(crc8(buffer_check, 0, pkt_start + pkt_len, test_value) == buffer_check[pkt_start + pkt_len])
                {
                    printf("match: 0/%02d 0x%02X\n", pkt, (uint32_t)test_value);
                }
            }
        }
    }
}

/*
                ch @ hop
>  FSK, DA3A0B, 49 @ 61 Rx  03 A2 F8 E7 DF 7F A1 84 12 4A 28 00 00 00 00 00 00 00 00 00 00 E0 07
>  FSK, DD7A0B, 49 @ 61 Tx  03 4C 5F 00 00 00 00 00 00 00 00 00 C4 crci: 82
>  FSK, DA0820, 46 @ 62 Rx  03 A2 FC E7 1F 80 A1 84 12 4A 28 00 00 00 00 00 00 00 00 00 00 E7 45
>  FSK, DD4820, 46 @ 62 Tx  03 5F 04 00 00 00 00 00 00 00 00 00 D5 crci: A6
>  FSK, D7A072, 09 @ 63 Rx  03 A2 F8 E7 DF 7F A1 84 12 4A 28 70 DA 0F 02 1D 4A 61 CF 00 00 46 CC
>  FSK, DAE072, 09 @ 63 Tx  03 C7 96 00 00 00 00 00 00 00 00 00 E9 crci: D8
>  FSK, D80449, 15 @ 64 Rx  03 A2 F8 E7 DF 7F A1 84 12 4A 28 00 00 00 00 00 00 00 00 00 00 3F CD
>  FSK, DB4449, 15 @ 64 Tx  83 00 6A 00 00 00 00 00 00 00 00 00 ED crci: 38
>  FSK, D9E6D9, 44 @ 65 Rx  03 A2 F8 E7 1F 80 A1 84 12 4A 28 00 00 00 00 00 00 00 00 00 00 4A 42
>  FSK, DD26D9, 44 @ 65 Tx  03 44 5F 00 00 00 00 00 00 00 00 00 F1 crci: A8
>  FSK, DA2968, 48 @ 66 Rx  03 A2 FC E7 1F 80 A1 84 12 4A 28 00 00 00 00 00 00 00 00 00 00 C8 6C
>  FSK, DD6968, 48 @ 66 Tx  03 5F 04 00 00 00 00 00 00 00 00 00 2C crci: 77

*/

uint8_t packets_crc8_payload[][13] = {
    { 0x03, 0x4C, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC4 },
    { 0x03, 0x5F, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xD5 },
    { 0x03, 0xC7, 0x96, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE9 },
    { 0x83, 0x00, 0x6A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xED },
    { 0x03, 0x44, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF1 },
    { 0x03, 0x5F, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2C }
};

uint8_t packets_crc8_hopnum[] = { 61, 62, 63, 64, 65, 66 };

void brute_crc8_payload()
{
    uint8_t buffer[32];
    int pkt_start = 4;
    int pkt_len = 12;

    #define off(x) ( ((x) + omp_get_thread_num())%4)

    printf("thread #%d\n", omp_get_thread_num());
    memcpy(&buffer[pkt_start], packets_crc8_payload[0], pkt_len + 1);

    for(uint64_t test_value = 0; test_value < 0x100000000; test_value++)
    {
        buffer[off(0)] = test_value >> 24;
        buffer[off(1)] = test_value >> 16;
        buffer[off(2)] = test_value >> 8;
        buffer[off(3)] = test_value + packets_crc8_hopnum[0];

        if(crc8(buffer, 0, pkt_start + pkt_len, 0) == buffer[pkt_start + pkt_len])
        {
            uint8_t buffer_check[32];
            uint8_t valid = 1;

            for(int pkt = 1; pkt < COUNT(packets_crc8_payload); pkt++)
            {
                buffer_check[off(0)] = test_value >> 24;
                buffer_check[off(1)] = test_value >> 16;
                buffer_check[off(2)] = test_value >> 8;
                buffer_check[off(3)] = test_value + packets_crc8_hopnum[pkt];

                memcpy(&buffer_check[pkt_start], packets_crc8_payload[pkt], pkt_len + 1);
                if(crc8(buffer_check, 0, pkt_start + pkt_len, 0) == buffer_check[pkt_start + pkt_len])
                {

                }
                else
                {
                    valid = 0;
                }
            }

            if(valid)
            {
                printf("match: 0x%08X\n", (uint32_t)test_value);
            }
        }
    }
}


void brute_crc16()
{
    uint8_t buffer[32];
    int pkt_start = 0;
    int pkt_len = 21;

    memcpy(&buffer[pkt_start], packets_crc16[2], pkt_len + 2);

    for(uint64_t test_value = 0; test_value < 0x10000; test_value++)
    {
        uint16_t crc = crc16(buffer, 0, pkt_start + pkt_len, test_value);
        uint16_t crct = *((uint16_t*)&buffer[pkt_start + pkt_len]);

        if(crc == crct)
        {
            uint8_t buffer_check[32];

            for(int pkt = 1; pkt < 21; pkt++)
            {
                memcpy(&buffer_check[pkt_start], packets_crc16[pkt], pkt_len + 2);
                uint16_t ccrc = crc16(buffer_check, 0, pkt_start + pkt_len, test_value);
                uint16_t ccrct = *((uint16_t*)&buffer_check[pkt_start + pkt_len]);
                
                if(ccrc == ccrct)
                {
                    printf("match: 0/%d 0x%08X\n", pkt, (uint32_t)test_value);
                }
            }
        }
    }
}




void brute_crc16_header()
{
    uint8_t buffer[32];
    int pkt_start = 3;
    int pkt_len = 21;
    uint16_t crc_init = 0xFFFF;

    memcpy(&buffer[pkt_start], packets_crc16[1], pkt_len + 2);

    for(uint64_t test_value = 0; test_value < 0x10000; test_value++)
    {
        buffer[0] = test_value;
        buffer[1] = test_value >> 8;
        //buffer[2] = test_value >> 16;
        //buffer[3] = test_value >> 24;

        uint16_t crc = crc16(buffer, 0, pkt_start + pkt_len, crc_init);
        uint16_t crct = *((uint16_t*)&buffer[pkt_start + pkt_len]);

        if(crc == crct)
        {
            uint8_t buffer_check[32];

            for(int pkt = 1; pkt < 21; pkt++)
            {
                buffer_check[0] = test_value;
                buffer_check[1] = test_value >> 8;
                //buffer_check[2] = test_value >> 16;
                //buffer_check[3] = test_value >> 24;

                memcpy(&buffer_check[pkt_start], packets_crc16[pkt], pkt_len + 2);
                uint16_t ccrc = crc16(buffer_check, 0, pkt_start + pkt_len, crc_init);
                uint16_t ccrct = *((uint16_t*)&buffer_check[pkt_start + pkt_len]);
                
                if(ccrc == ccrct)
                {
                    printf("match: 0/%d 0x%08X\n", pkt, (uint32_t)test_value);
                }
            }
        }
    }
}



int main(int argc, char *argv[])
{    
    #pragma omp parallel num_threads(4)
    brute_crc8_payload();
    return 0;
}